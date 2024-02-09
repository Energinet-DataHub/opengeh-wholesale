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
from pyspark.sql import SparkSession

from package.calculation_input.schemas import metering_point_period_schema


class TableReaderMockBuilder:

    table_reader: Mock

    def __init__(self, spark: SparkSession):
        self.spark = spark
        self.table_reader = Mock()
        test_dir = os.path.dirname(os.path.abspath(__file__))
        self.test_path = os.path.join(test_dir, "test_data/")


    def populate_metering_point_periods(self, file_path: str) -> None:
        df = self.spark.read.csv(self.test_path + file_path, header=True, schema=metering_point_period_schema)
        new_df = self.spark.createDataFrame(df.rdd, metering_point_period_schema)
        self.table_reader.read_metering_point_periods.return_value = new_df

    def get_table_reader(self) -> Mock:
        return self.table_reader