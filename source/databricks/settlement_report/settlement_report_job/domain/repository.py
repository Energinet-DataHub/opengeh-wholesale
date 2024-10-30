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
from datetime import datetime
from uuid import UUID

from pyspark.sql import DataFrame, SparkSession, functions as F

from settlement_report_job.domain.dataframe_utils.factory_filters import (
    filter_by_calculation_id_by_grid_area,
)
from settlement_report_job.wholesale.column_names import DataProductColumnNames
from settlement_report_job.wholesale.database_definitions import (
    WholesaleResultsDatabase,
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

    def read_metering_point_periods(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleBasisDataDatabase.DATABASE_NAME,
            WholesaleBasisDataDatabase.METERING_POINT_PERIODS_VIEW_NAME,
        )

    def read_filtered_metering_point_periods(
        self,
        period_start: datetime,
        period_end: datetime,
        energy_supplier_ids: list[str] | None,
        calculation_id_by_grid_area: dict[str, UUID],
    ) -> DataFrame:
        metering_point_periods = self.read_metering_point_periods().where(
            (F.col(DataProductColumnNames.from_date) < period_end)
            & (F.col(DataProductColumnNames.to_date) > period_start)
        )

        metering_point_periods = metering_point_periods.where(
            filter_by_calculation_id_by_grid_area(calculation_id_by_grid_area)
        )

        if energy_supplier_ids is not None:
            metering_point_periods = metering_point_periods.where(
                F.col(DataProductColumnNames.energy_supplier_id).isin(
                    energy_supplier_ids
                )
            )

        return metering_point_periods

    def read_metering_point_time_series(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleBasisDataDatabase.DATABASE_NAME,
            WholesaleBasisDataDatabase.TIME_SERIES_POINTS_VIEW_NAME,
        )

    def read_charge_link_periods(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleBasisDataDatabase.DATABASE_NAME,
            WholesaleBasisDataDatabase.CHARGE_LINKS_VIEW_NAME,
        )

    def read_charge_price_information_periods(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleBasisDataDatabase.DATABASE_NAME,
            WholesaleBasisDataDatabase.CHARGE_PRICE_INFORMATION_PERIODS_VIEW_NAME,
        )

    def read_energy(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleResultsDatabase.DATABASE_NAME,
            WholesaleResultsDatabase.ENERGY_V1_VIEW_NAME,
        )

    def read_latest_calculations(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleResultsDatabase.DATABASE_NAME,
            WholesaleResultsDatabase.LATEST_CALCULATIONS_BY_DAY_VIEW_NAME,
        )

    def read_energy_per_es(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleResultsDatabase.DATABASE_NAME,
            WholesaleResultsDatabase.ENERGY_PER_ES_V1_VIEW_NAME,
        )

    def read_amounts_per_charge(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleResultsDatabase.DATABASE_NAME,
            WholesaleResultsDatabase.AMOUNTS_PER_CHARGE_VIEW_NAME,
        )

    def _read_view_or_table(
        self,
        database_name: str,
        table_name: str,
    ) -> DataFrame:
        name = f"{self._catalog_name}.{database_name}.{table_name}"
        return self._spark.read.format("delta").table(name)
