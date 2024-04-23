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

from pyspark.sql import SparkSession, dataframe

from package.infrastructure import paths
from package.infrastructure.paths import (
    METERING_POINT_PERIODS_SETTLEMENT_REPORT_VIEW_NAME_V1,
    SETTLEMENT_REPORT_DATABASE_NAME,
    METERING_POINT_TIME_SERIES_SETTLEMENT_REPORT_VIEW_NAME_V1,
)


class SettlementReportViewReader:
    """
    This class is responsible for retrieving data from settlement report views.
    It is only used in tests.
    """

    def __init__(
        self,
        spark: SparkSession,
        metering_point_periods_view_name: str | None = None,
        metering_point_time_series_view_name: str | None = None,
    ) -> None:
        self._spark = spark
        self._metering_point_periods_view_name = (
            metering_point_periods_view_name
            or paths.METERING_POINT_PERIODS_SETTLEMENT_REPORT_VIEW_NAME_V1
        )
        self._metering_point_time_series_view_name = (
            metering_point_time_series_view_name
            or paths.METERING_POINT_TIME_SERIES_SETTLEMENT_REPORT_VIEW_NAME_V1
        )

    def read_metering_point_periods(
        self,
    ) -> dataframe:
        return self._spark.read.format("delta").table(
            f"{SETTLEMENT_REPORT_DATABASE_NAME}.{METERING_POINT_PERIODS_SETTLEMENT_REPORT_VIEW_NAME_V1}"
        )

    def read_metering_point_time_series(
        self,
    ) -> dataframe:
        return self._spark.read.format("delta").table(
            f"{SETTLEMENT_REPORT_DATABASE_NAME}.{METERING_POINT_TIME_SERIES_SETTLEMENT_REPORT_VIEW_NAME_V1}"
        )
