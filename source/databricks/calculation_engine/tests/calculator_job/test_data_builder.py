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
import os
from unittest.mock import Mock

from pyspark.sql import SparkSession, DataFrame

from package.calculation import PreparedDataReader
from package.calculation_input.schemas import (
    metering_point_period_schema,
    time_series_point_schema,
    grid_loss_metering_points_schema,
)


class TableReaderMockBuilder:

    table_reader: Mock

    def __init__(self, spark: SparkSession, test_path: str):
        self.spark = spark
        self.table_reader = Mock()
        test_dir = os.path.dirname(os.path.abspath(__file__))
        self.test_path = os.path.join(test_dir, test_path)

    def populate_metering_point_periods(self, file_path: str) -> None:
        df = self._parse_csv_to_dataframe(file_path, metering_point_period_schema)
        self.table_reader.read_metering_point_periods.return_value = df

    def populate_time_series_points(self, file_path: str) -> None:
        df = self._parse_csv_to_dataframe(file_path, time_series_point_schema)
        self.table_reader.read_time_series_points.return_value = df

    def populate_grid_loss_metering_points(self, file_path: str) -> None:
        df = self._parse_csv_to_dataframe(file_path, grid_loss_metering_points_schema)
        self.table_reader.read_grid_loss_metering_points.return_value = df

    def _parse_csv_to_dataframe(self, file_path: str, schema: str) -> DataFrame:
        df = self.spark.read.csv(self.test_path + file_path, header=True, schema=schema)
        return self.spark.createDataFrame(df.rdd, schema)

    def create_prepared_data_reader(self) -> PreparedDataReader:
        return PreparedDataReader(self.table_reader)
