# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
import concurrent.futures
import os
from pathlib import Path
from typing import Tuple
from unittest.mock import Mock

from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import StructType
import telemetry_logging.logging_configuration as config

from package.calculation.calculation_output import CalculationOutput
from package.calculation.calculator_args import CalculatorArgs
from .calculation_args import create_calculation_args
from .dataframes.typecasting import cast_column_types
from .expected_output import ExpectedOutput
from .input_specifications import get_data_input_specifications


class ScenarioExecutor:
    migrations_wholesale_repository: Mock
    test_calculation_args: CalculatorArgs
    input_path: str
    output_path: str

    def __init__(self, spark: SparkSession):
        self.spark = spark
        self.migrations_wholesale_repository = Mock()
        self.wholesale_internal_repository = Mock()
        self.wholesale_results_internal_repository = Mock()

    def execute(
        self, scenario_folder_path: str
    ) -> Tuple[CalculationOutput, list[ExpectedOutput]]:
        self._setup(scenario_folder_path)

        from package.calculation import PreparedDataReader
        from package.calculation.calculation_core import CalculationCore

        actual = CalculationCore().execute(
            self.test_calculation_args,
            PreparedDataReader(
                self.migrations_wholesale_repository,
                self.wholesale_internal_repository,
            ),
        )
        expected = self._get_expected_results(self.spark)
        return actual, expected

    def _setup(self, scenario_path: str) -> None:
        self.input_path = scenario_path + "/when/"
        self.output_path = scenario_path + "/then/"

        correlations = get_data_input_specifications(
            self.migrations_wholesale_repository,
            self.wholesale_internal_repository,
            self.wholesale_results_internal_repository,
        )

        config.configure_logging(
            cloud_role_name="dbr-calculation-engine-tests",
            tracer_name="feature-tests",
        )

        self.test_calculation_args = create_calculation_args(self.input_path)
        dataframes = self._read_files_in_parallel(correlations)

        for i, (_, reader) in enumerate(correlations.values()):
            if reader is not None:
                reader.return_value = dataframes[i]

    def _read_file(
        self, spark_session: SparkSession, csv_file_name: str, schema: StructType
    ) -> DataFrame | None:

        path_to_csv = self.input_path + csv_file_name

        if not os.path.exists(path_to_csv):
            return None

        # Reading the CSV file without applying the schema means all types are strings.
        # We do this because some types are not supported by the CSV reader fx. ArrayType.
        df = spark_session.read.csv(path_to_csv, header=True, sep=";")

        # Cast the column types to match the schema
        df = cast_column_types(df, table_or_view_name=csv_file_name)

        # Verify column names match with those in the schema
        schema_column_names = [field.name for field in schema.fields]
        if schema_column_names != df.columns:
            raise ValueError(
                f"Schema mismatch {path_to_csv}. Expected: {schema_column_names} Found: {df.columns}"
            )

        return spark_session.createDataFrame(df.rdd, schema=schema, verifySchema=True)

    def _read_files_in_parallel(
        self, correlations: dict[str, tuple]
    ) -> list[DataFrame]:
        """
        Reads all the csv files in parallel and converts them to dataframes.
        """
        schemas = [t[0] for t in correlations.values()]

        with concurrent.futures.ThreadPoolExecutor() as executor:
            dataframes = list(
                executor.map(
                    self._read_file,
                    [self.spark] * len(correlations.keys()),
                    correlations.keys(),
                    schemas,
                )
            )
        return dataframes

    def _get_expected_results(self, spark: SparkSession) -> list[ExpectedOutput]:
        expected_results = []
        expected_result_file_paths = (
            self._get_paths_to_expected_result_files_in_output_folder()
        )

        if len(expected_result_file_paths) == 0:
            raise Exception("Missing expected result files in output folder.")

        for result_file in expected_result_file_paths:
            raw_df = spark.read.csv(result_file[1], header=True, sep=";")
            df = cast_column_types(raw_df, table_or_view_name=result_file[0])
            expected_results.append(ExpectedOutput(name=result_file[0], df=df))

        return expected_results

    def _get_paths_to_expected_result_files_in_output_folder(
        self,
    ) -> list[Tuple[str, str]]:
        """Returns (file base name without extension, file full path)."""

        output_folder_path = Path(self.output_path)
        csv_files = list(output_folder_path.rglob("*.csv"))
        return [(Path(file).stem, str(file)) for file in csv_files]
