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
from importlib.util import spec_from_file_location, module_from_spec
from typing import Callable
from unittest.mock import Mock

from pyspark.sql import SparkSession, DataFrame

from calculation_logic.utils.correlations import get_correlations
from calculation_logic.utils.create_calculation_args import create_calculation_args
from package.calculation import PreparedDataReader
from package.calculation.calculation import _execute
from package.calculation.calculation_results import (
    CalculationResultsContainer,
)
from package.calculation.calculator_args import CalculatorArgs


class ScenarioFixture:

    table_reader: Mock
    calculation_args: CalculatorArgs
    test_path: str
    parent_dir: str
    results: DataFrame
    expected: DataFrame
    correlations = dict[str, tuple]

    def __init__(self, spark: SparkSession):
        self.spark = spark
        self.table_reader = Mock()

    def setup(self, get_expected_result: Callable[..., DataFrame]) -> None:
        file_path = inspect.getfile(get_expected_result)
        self.parent_dir = os.path.dirname(os.path.dirname(file_path))
        self.test_path = self.parent_dir + "/test_data/"

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
        self, spark_session: SparkSession, csv_file_name: str, schema: str
    ) -> DataFrame:

        path_to_csv = self.test_path + csv_file_name

        if not os.path.exists(path_to_csv):
            return self.create_test_dataframe_using_factory(
                csv_file_name, spark_session, schema
            )

        df = spark_session.read.csv(path_to_csv, header=True, sep=";", schema=schema)

        # Because the expected result csv contains unsupported types (array type) it can't be
        # parsed with a schema. Therefore, all types are converted the string type. It also
        # means that the dataframe need type changes before applying the schema.
        if csv_file_name.__contains__("expected_results.csv"):
            return df

        # We need to create the dataframe again because nullability and precision
        # are not applied when reading the csv file.
        return spark_session.createDataFrame(df.rdd, schema)

    def create_test_dataframe_using_factory(
        self, file_path, spark_session, schema: str
    ) -> DataFrame:
        spec = spec_from_file_location(
            "module.name", self.parent_dir + "/states/scenario_state.py"
        )
        module = module_from_spec(spec)
        spec.loader.exec_module(module)
        method_name = "create_" + os.path.splitext(file_path)[0]
        if hasattr(module, method_name):
            method_create = getattr(module, method_name)
            return method_create(spark_session)

        # When factory method doesn't exist return an empty dataframe
        return spark_session.createDataFrame([], schema)

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
