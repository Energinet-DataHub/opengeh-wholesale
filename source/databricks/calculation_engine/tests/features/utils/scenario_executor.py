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

from package.calculation.calculation_results import CalculationResultsContainer
from package.calculation.calculator_args import CalculatorArgs
from .calculation_args import create_calculation_args
from .dataframes import (
    create_energy_result_dataframe,
    create_wholesale_result_dataframe,
    create_basis_data_result_dataframe,
    create_total_monthly_amounts_dataframe,
)
from .expected_output import ExpectedOutput
from .input_specifications import get_data_input_specifications


class ScenarioExecutor:
    table_reader: Mock
    test_calculation_args: CalculatorArgs
    input_path: str
    output_path: str
    basis_data_path: str

    def __init__(self, spark: SparkSession):
        self.spark = spark
        self.table_reader = Mock()

    def execute(
        self, scenario_folder_path: str
    ) -> Tuple[CalculationResultsContainer, list[ExpectedOutput]]:
        self._setup(scenario_folder_path)

        from package.calculation import PreparedDataReader
        from package.calculation.calculation import _execute

        actual = _execute(
            self.test_calculation_args, PreparedDataReader(self.table_reader)
        )
        expected = self._get_expected_results(self.spark)
        return actual, expected

    def _setup(self, scenario_path: str) -> None:
        self.input_path = scenario_path + "/input/"
        self.basis_data_path = scenario_path + "/basis_data/"
        self.output_path = scenario_path + "/output/"

        correlations = get_data_input_specifications(self.table_reader)
        self.test_calculation_args = create_calculation_args(self.input_path)
        dataframes = self._read_files_in_parallel(correlations)

        for i, (_, reader) in enumerate(correlations.values()):
            if reader is not None:
                reader.return_value = dataframes[i]

    def _read_file(
        self, spark_session: SparkSession, csv_file_name: str, schema: str
    ) -> DataFrame | None:

        path_to_csv = self.input_path + csv_file_name

        if not os.path.exists(path_to_csv):
            return None

        df = spark_session.read.csv(path_to_csv, header=True, sep=";", schema=schema)

        # We need to create the dataframe again because nullability and precision
        # are not applied when reading the csv file.
        return spark_session.createDataFrame(df.rdd, schema)

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
            if "energy_results" in result_file[1]:
                df = create_energy_result_dataframe(spark, raw_df)
            elif "wholesale_results" in result_file[1]:
                df = create_wholesale_result_dataframe(spark, raw_df)
            elif "total_monthly_amounts" in result_file[1]:
                df = create_total_monthly_amounts_dataframe(spark, raw_df)
            elif "basis_data" in result_file[1]:
                df = create_basis_data_result_dataframe(spark, raw_df, result_file[0])
            else:
                raise Exception(f"Unsupported result file '{result_file[0]}'")
            expected_results.append(ExpectedOutput(name=result_file[0], df=df))

        return expected_results

    def _get_paths_to_expected_result_files_in_output_folder(
        self,
    ) -> list[Tuple[str, str]]:
        """Returns (file base name without extension, file full path)."""

        output_folder_path = Path(self.output_path)
        csv_files = list(output_folder_path.rglob("*.csv"))
        return [(Path(file).stem, str(file)) for file in csv_files]
