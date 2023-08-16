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

from pyspark.sql import DataFrame, SparkSession
from package.constants import INPUT_DATABASE_NAME, 

class CalculationInputReader:
    def __init__(
        self,
        spark: SparkSession,
        wholesale_container_path: str,
    ) -> None:
        self.__spark = spark
        self.__wholesale_container_path = wholesale_container_path

    def read_metering_point_periods(self) -> DataFrame:
        return (
            self.__spark.read.option("mode", "FAILFAST").format("delta")
            .load(f"{self.__wholesale_container_path}/calculation_input/metering_point_periods")
        )

    def read_time_series_points(self) -> DataFrame:
        return (
            self.__spark.read.option("mode", "FAILFAST").format("delta")
            .load(f"{self.__wholesale_container_path}/calculation_input/time_series_points")
        )

    def read_charge_links_periods(self) -> DataFrame:
        return self.__spark.read.table(f"{DATABASE_NAME}.{WHOLESALE_TABLE_NAME}")
