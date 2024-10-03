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

from settlement_report_job.infrastructure.database_definitions import (
    WholesaleSettlementReportDatabase,
    WholesaleWholesaleResultsDatabase,
    WholesaleBasisDataDatabase,
)


class WholesaleRepository:
    def __init__(
        self,
        spark: SparkSession,
        catalog_name: str,
    ) -> None:
        self._spark = spark
        self._catalog_name = catalog_name

    def read_metering_point_time_series(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleSettlementReportDatabase.DATABASE_NAME,
            WholesaleSettlementReportDatabase.METERING_POINT_TIME_SERIES_VIEW_NAME,
        )

    def read_charge_link_periods(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleBasisDataDatabase.DATABASE_NAME,
            WholesaleBasisDataDatabase.CHARGE_LINKS_VIEW_NAME,
        )

    def read_charge_price_information_periods(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleBasisDataDatabase.DATABASE_NAME,
            WholesaleBasisDataDatabase.CHARGE_LINKS_VIEW_NAME,
        )

    def read_energy(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleWholesaleResultsDatabase.DATABASE_NAME,
            WholesaleWholesaleResultsDatabase.ENERGY_V1_VIEW_NAME,
        )

    def _read_view_or_table(
        self,
        database_name: str,
        table_name: str,
    ) -> DataFrame:
        name = f"{self._catalog_name}.{database_name}.{table_name}"
        return self._spark.read.format("delta").table(name)
