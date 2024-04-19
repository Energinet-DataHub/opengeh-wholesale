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

from package.infrastructure import paths
from package.infrastructure.paths import (
    METERING_POINT_PERIODS_SETTLEMENT_REPORT_VIEW_NAME_V1,
    SETTLEMENT_REPORT_DATABASE_NAME,
)


class ViewReader:
    def __init__(
        self,
        spark: SparkSession,
        schema_path: str,
        metering_point_periods_view_name: str | None = None,
    ) -> None:
        self._spark = spark
        self._schema_path = schema_path
        self._metering_point_periods_view_name = (
            metering_point_periods_view_name
            or paths.METERING_POINT_PERIODS_SETTLEMENT_REPORT_VIEW_NAME_V1
        )

    def read_metering_point_periods(
        self,
    ) -> DataFrame:
        return self._spark.read.format("delta").table(
            f"{SETTLEMENT_REPORT_DATABASE_NAME}.{METERING_POINT_PERIODS_SETTLEMENT_REPORT_VIEW_NAME_V1}"
        )
