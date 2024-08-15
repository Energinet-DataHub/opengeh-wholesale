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

import pyspark.sql.functions as f
from pyspark.sql import DataFrame

from package.calculation.preparation.data_structures.grid_loss_responsible import (
    GridLossResponsible,
)
from package.calculation.preparation.data_structures.input_charges import (
    InputChargesContainer,
)
from package.calculation.preparation.data_structures.prepared_charges import (
    PreparedChargesContainer,
)
from package.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from package.codelists import ChargeResolution, CalculationType
from package.databases.migrations_wholesale import MigrationsWholesaleRepository

from . import transformations as T
from ...constants import Colname
from ...databases import wholesale_internal
from ...databases.table_column_names import TableColumnNames
from ...infrastructure import logging_configuration


class PreparedDataReader:
    def __init__(
        self,
        migrations_wholesale_repository: MigrationsWholesaleRepository,
        wholesale_internal_repository: wholesale_internal.WholesaleInternalRepository,
    ) -> None:
        self._migrations_wholesale_repository = migrations_wholesale_repository
        self._wholesale_internal_repository = wholesale_internal_repository

    @logging_configuration.use_span("get_metering_point_periods")
    def get_metering_point_periods_df(
        self,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
        grid_areas: list[str],
    ) -> DataFrame:
        return T.get_metering_point_periods_df(
            self._migrations_wholesale_repository,
            period_start_datetime,
            period_end_datetime,
            grid_areas,
        )

    @logging_configuration.use_span("get_grid_loss_responsible")
    def get_grid_loss_responsible(
        self, grid_areas: list[str], metering_point_periods_df: DataFrame
    ) -> GridLossResponsible:
        return T.get_grid_loss_responsible(
            grid_areas, metering_point_periods_df, self._wholesale_internal_repository
        )

    @logging_configuration.use_span("get_metering_point_time_series")
    def get_metering_point_time_series(
        self,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
        metering_point_periods_df_without_grid_loss: DataFrame,
    ) -> PreparedMeteringPointTimeSeries:
        time_series_points_df = T.get_time_series_points(
            self._migrations_wholesale_repository,
            period_start_datetime,
            period_end_datetime,
        )
        return T.get_metering_point_time_series(
            time_series_points_df,
            metering_point_periods_df_without_grid_loss,
        )

    @logging_configuration.use_span("get_input_charges")
    def get_input_charges(
        self,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
    ) -> InputChargesContainer:
        charge_price_information = T.read_charge_price_information(
            self._migrations_wholesale_repository,
            period_start_datetime,
            period_end_datetime,
        )

        charge_prices = T.read_charge_prices(
            self._migrations_wholesale_repository,
            period_start_datetime,
            period_end_datetime,
        )

        charge_links = T.read_charge_links(
            self._migrations_wholesale_repository,
            period_start_datetime,
            period_end_datetime,
        )

        return InputChargesContainer(
            charge_price_information=charge_price_information,
            charge_prices=charge_prices,
            charge_links=charge_links,
        )

    def get_prepared_charges(
        self,
        metering_point_periods: DataFrame,
        time_series: PreparedMeteringPointTimeSeries,
        input_charges: InputChargesContainer,
        time_zone: str,
    ) -> PreparedChargesContainer:
        charge_link_metering_point_periods = T.get_charge_link_metering_point_periods(
            input_charges.charge_links, metering_point_periods
        )

        hourly_tariffs = T.get_prepared_tariffs(
            time_series,
            input_charges.charge_price_information,
            input_charges.charge_prices,
            charge_link_metering_point_periods,
            ChargeResolution.HOUR,
            time_zone,
        )

        daily_tariffs = T.get_prepared_tariffs(
            time_series,
            input_charges.charge_price_information,
            input_charges.charge_prices,
            charge_link_metering_point_periods,
            ChargeResolution.DAY,
            time_zone,
        )

        subscriptions = T.get_prepared_subscriptions(
            input_charges.charge_price_information,
            input_charges.charge_prices,
            charge_link_metering_point_periods,
            time_zone,
        )

        fees = T.get_prepared_fees(
            input_charges.charge_price_information,
            input_charges.charge_prices,
            charge_link_metering_point_periods,
            time_zone,
        )

        return PreparedChargesContainer(
            hourly_tariffs=hourly_tariffs,
            daily_tariffs=daily_tariffs,
            subscriptions=subscriptions,
            fees=fees,
        )

    def get_metering_point_periods_without_grid_loss(
        self, metering_point_periods_df: DataFrame
    ) -> DataFrame:
        # Remove grid loss metering point periods
        return metering_point_periods_df.join(
            self._wholesale_internal_repository.read_grid_loss_metering_points(),
            Colname.metering_point_id,
            "left_anti",
        )

    def get_latest_calculation_version(
        self, calculation_type: CalculationType
    ) -> int | None:
        """Returns the latest used version for the selected calculation type or None."""

        calculations = self._wholesale_internal_repository.read_calculations()

        latest_version = (
            calculations.where(
                f.col(TableColumnNames.calculation_type) == calculation_type.value
            )
            .agg(
                f.max(TableColumnNames.calculation_version).alias(
                    TableColumnNames.calculation_version
                )
            )
            .collect()[0][TableColumnNames.calculation_version]
        )

        return latest_version
