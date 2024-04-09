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
from dataclasses import dataclass
from pathlib import Path
from typing import Tuple
from unittest.mock import Mock

from pyspark.sql import SparkSession, DataFrame

from features.correlations import get_correlations
from features.test_calculation_args import create_calculation_args
from features.utils import create_wholesale_result_dataframe
from features.utils.dataframes.energy_results_dataframe import (
    create_energy_result_dataframe,
)
from package.calculation import PreparedDataReader
from package.calculation.calculation import _execute
from package.calculation.calculation_results import (
    CalculationResultsContainer,
)
from package.calculation.calculator_args import CalculatorArgs


@dataclass
class ExpectedResult:
    name: str
    df: DataFrame


class ScenarioFixture2:
    table_reader: Mock
    test_calculation_args: CalculatorArgs
    input_path: str
    scenario_path: str
    correlations = dict[str, tuple]

    def __init__(self, spark: SparkSession):
        self.spark = spark
        self.table_reader = Mock()

    def execute(
        self, scenario_folder_path: str
    ) -> Tuple[CalculationResultsContainer, list[ExpectedResult]]:
        self._setup(scenario_folder_path)
        actual = _execute(
            self.test_calculation_args, PreparedDataReader(self.table_reader)
        )
        expected = self._get_expected_results(self.spark)
        return actual, expected

    def _setup(self, scenario_path: str) -> None:
        self.scenario_path = scenario_path
        self.input_path = self.scenario_path + "/input/"

        correlations = get_correlations(self.table_reader)
        self.test_calculation_args = create_calculation_args(self.input_path)
        dataframes = self._read_files_in_parallel(correlations)

        for i, (_, reader) in enumerate(correlations.values()):
            if reader is not None:
                reader.return_value = dataframes[i]

    def _read_file(
        self, spark_session: SparkSession, csv_file_name: str, schema: str
    ) -> DataFrame:

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

    def _get_expected_results(self, spark: SparkSession) -> list[ExpectedResult]:
        expected_results = []
        expected_result_file_paths = (
            self._get_paths_to_expected_result_files_in_output_folder()
        )

        if len(expected_result_file_paths) == 0:
            raise Exception("Missing expected result files in output folder.")

        for result_file in expected_result_file_paths:
            raw_df = spark.read.csv(result_file[1], header=True, sep=";")
            if "energy_results" in result_file[1]:
                df = create_energy_result_dataframe(
                    spark, raw_df, self.test_calculation_args
                )
            elif "wholesale_results" in result_file[1]:
                df = create_wholesale_result_dataframe(
                    spark, raw_df, self.test_calculation_args
                )
            else:
                raise Exception(f"Unsupported result file '{result_file[0]}'")
            expected_results.append(ExpectedResult(name=result_file[0], df=df))

        return expected_results

    def _get_paths_to_expected_result_files_in_output_folder(
        self,
    ) -> list[Tuple[str, str]]:
        """Returns (file base name without extension, file full path)."""

        output_folder_path = Path(f"{self.scenario_path}/output")
        csv_files = list(output_folder_path.rglob("*.csv"))
        return [(Path(file).stem, str(file)) for file in csv_files]
