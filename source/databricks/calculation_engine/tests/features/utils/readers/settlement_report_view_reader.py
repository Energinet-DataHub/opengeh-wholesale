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

from package.infrastructure.paths import (
    METERING_POINT_PERIODS_SETTLEMENT_REPORT_VIEW_NAME_V1,
    SETTLEMENT_REPORT_DATABASE_NAME,
    METERING_POINT_TIME_SERIES_SETTLEMENT_REPORT_VIEW_NAME_V1,
    ENERGY_RESULTS_SETTLEMENT_REPORT_VIEW_NAME_V1,
    CHARGE_LINK_PERIODS_SETTLEMENT_REPORT_VIEW_NAME_V1,
    CHARGE_PRICES_SETTLEMENT_REPORT_VIEW_NAME_V1,
    WHOLESALE_RESULTS_SETTLEMENT_REPORT_VIEW_NAME_V1,
)


class SettlementReportViewReader:
    """
    This class is responsible for retrieving data from settlement report views.
    """

    @staticmethod
    def read_metering_point_periods_v1(spark: SparkSession) -> DataFrame:
        return spark.read.format("delta").table(
            f"{SETTLEMENT_REPORT_DATABASE_NAME}.{METERING_POINT_PERIODS_SETTLEMENT_REPORT_VIEW_NAME_V1}"
        )

    @staticmethod
    def read_metering_point_time_series_v1(spark: SparkSession) -> DataFrame:
        return spark.read.format("delta").table(
            f"{SETTLEMENT_REPORT_DATABASE_NAME}.{METERING_POINT_TIME_SERIES_SETTLEMENT_REPORT_VIEW_NAME_V1}"
        )

    @staticmethod
    def read_charge_link_periods_v1(spark: SparkSession) -> DataFrame:
        return spark.read.format("delta").table(
            f"{SETTLEMENT_REPORT_DATABASE_NAME}.{CHARGE_LINK_PERIODS_SETTLEMENT_REPORT_VIEW_NAME_V1}"
        )

    @staticmethod
    def read_charge_prices_v1(spark: SparkSession) -> DataFrame:
        return spark.read.format("delta").table(
            f"{SETTLEMENT_REPORT_DATABASE_NAME}.{CHARGE_PRICES_SETTLEMENT_REPORT_VIEW_NAME_V1}"
        )

    @staticmethod
    def read_energy_results_v1(spark: SparkSession) -> DataFrame:
        return spark.read.format("delta").table(
            f"{SETTLEMENT_REPORT_DATABASE_NAME}.{ENERGY_RESULTS_SETTLEMENT_REPORT_VIEW_NAME_V1}"
        )

    @staticmethod
    def read_wholesale_results_v1(spark: SparkSession) -> DataFrame:
        return spark.read.format("delta").table(
            f"{SETTLEMENT_REPORT_DATABASE_NAME}.{WHOLESALE_RESULTS_SETTLEMENT_REPORT_VIEW_NAME_V1}"
        )
