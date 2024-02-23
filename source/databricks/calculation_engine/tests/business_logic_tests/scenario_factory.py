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
import inspect
import os
from typing import Callable
from unittest.mock import Mock

from pyspark.sql import SparkSession, DataFrame

from business_logic_tests.correlations import get_correlations
from business_logic_tests.create_calculation_args import create_calculation_args
from package.calculation import PreparedDataReader
from package.calculation.CalculationResults import (
    CalculationResultsContainer,
)
from package.calculation.calculation import _execute
from package.calculation.calculator_args import CalculatorArgs


class ScenarioFixture:

    table_reader: Mock
    calculation_args: CalculatorArgs
    test_path: str
    results: DataFrame
    expected: DataFrame

    def __init__(self, spark: SparkSession):
        self.spark = spark
        self.table_reader = Mock()

    def setup(self, get_expected_result: Callable[..., DataFrame]) -> None:
        file_path = inspect.getfile(get_expected_result)
        parent_dir = os.path.dirname(os.path.dirname(file_path))
        self.test_path = parent_dir + "/test_data/"

        correlations = get_correlations(self.table_reader)
        self.calculation_args = create_calculation_args(self.test_path)
        dataframes = self._read_files_in_parallel(correlations)

        for i, (_, reader) in enumerate(correlations.values()):
            if reader is not None:
                reader.return_value = dataframes[i]

        self.expected = get_expected_result(
            self.spark,
            dataframes[-1],
            self.calculation_args,
        )

    def execute(self) -> CalculationResultsContainer:
        return _execute(self.calculation_args, PreparedDataReader(self.table_reader))

    def _read_file(
        self, spark_session: SparkSession, file_path: str, schema: str
    ) -> DataFrame:

        path = self.test_path + file_path

        # if the file doesn't exist, return empty dataframe
        if not os.path.exists(path):
            return spark_session.createDataFrame([], schema)

        df = spark_session.read.csv(path, header=True, sep=";", schema=schema)

        # All the types in the expected dataframe (build from the expected results file) are
        # string types since the csv method doesn't support array types. These need to be convert
        # first to the correct types before applying the schema.
        if file_path.__contains__("expected_results.csv"):
            return df

        # We need to create the dataframe again because nullability and precision
        # are not applied when reading the csv file.
        return spark_session.createDataFrame(df.rdd, schema)

    def _read_files_in_parallel(
        self, correlations: dict[str, tuple]
    ) -> list[DataFrame]:
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
