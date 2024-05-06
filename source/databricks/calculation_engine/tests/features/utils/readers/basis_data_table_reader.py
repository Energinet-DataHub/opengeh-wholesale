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

from pyspark.sql import SparkSession, DataFrame

from package.infrastructure import paths
from package.infrastructure.paths import (
    BASIS_DATA_DATABASE_NAME,
)


class BasisDataTableReader:

    def __init__(
        self,
        spark: SparkSession,
    ) -> None:
        self._spark = spark

    def read_metering_point_periods(
        self,
    ) -> DataFrame:
        return self._spark.read.format("delta").table(
            f"{BASIS_DATA_DATABASE_NAME}.{paths.METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME}"
        )

    def read_time_series_points(
        self,
    ) -> DataFrame:
        return self._spark.read.format("delta").table(
            f"{BASIS_DATA_DATABASE_NAME}.{paths.TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME}"
        )

    def read_calculations(
        self,
    ) -> DataFrame:
        return self._spark.read.format("delta").table(
            f"{BASIS_DATA_DATABASE_NAME}.{paths.CALCULATIONS_TABLE_NAME}"
        )
