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

from business_logic_tests.load_calculation_args import load_calculation_args
from package.calculation import PreparedDataReader
from package.calculation.CalculationResults import (
    CalculationResultsContainer,
)
from package.calculation.calculation import _execute
from package.calculation.calculator_args import CalculatorArgs
from package.calculation_input.schemas import (
    metering_point_period_schema,
    time_series_point_schema,
    grid_loss_metering_points_schema,
    charge_link_periods_schema,
    charge_master_data_periods_schema,
    charge_price_points_schema,
)


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

        file_schema_dict = {
            "metering_point_periods.csv": metering_point_period_schema,
            "time_series_points.csv": time_series_point_schema,
            "grid_loss_metering_points.csv": grid_loss_metering_points_schema,
            "charge_master_data_periods.csv": charge_master_data_periods_schema,
            "charge_link_periods.csv": charge_link_periods_schema,
            "charge_price_points.csv": charge_price_points_schema,
            "expected_results.csv": None,
        }

        self.calculation_args = load_calculation_args(self.test_path)

        frames = self._read_files_in_parallel(
            list(file_schema_dict.keys()), list(file_schema_dict.values())
        )

        self.table_reader.read_metering_point_periods.return_value = frames[0]
        self.table_reader.read_time_series_points.return_value = frames[1]
        self.table_reader.read_grid_loss_metering_points.return_value = frames[2]
        self.table_reader.read_charge_master_data_periods.return_value = frames[3]
        self.table_reader.read_charge_links_periods.return_value = frames[4]
        self.table_reader.read_charge_price_points.return_value = frames[5]

        self.expected = get_expected_result(self.spark, self.calculation_args, frames[6])

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
        # string types. These need to be convert first to the correct types before applying
        # the schema.
        if file_path.__contains__("expected_results.csv"):
            return df

        # We need to create the dataframe again because nullability and precision
        # are not applied when reading the csv file.
        return spark_session.createDataFrame(df.rdd, schema)

    def _read_files_in_parallel(
        self, file_names: list, schemas: list
    ) -> list[DataFrame]:
        with concurrent.futures.ThreadPoolExecutor() as executor:
            dataframes = list(
                executor.map(
                    self._read_file, [self.spark] * len(file_names), file_names, schemas
                )
            )
        return dataframes
