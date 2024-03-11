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
from typing import Tuple

from pyspark.sql import DataFrame

from package.calculation.preparation.grid_loss_responsible import GridLossResponsible
from package.calculation.input import TableReader
from package.codelists import ChargeResolution
from . import transformations as T
from .charge_link_metering_point_periods import ChargeLinkMeteringPointPeriods
from .charge_master_data import ChargeMasterData
from .charge_prices import ChargePrices
from .prepared_tariffs import PreparedTariffs
from .prepared_subscriptions import PreparedSubscriptions
from ...infrastructure import logging_configuration


class PreparedDataReader:
    def __init__(self, delta_table_reader: TableReader) -> None:
        self._table_reader = delta_table_reader

    @logging_configuration.use_span("get_metering_point_periods")
    def get_metering_point_periods_df(
        self,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
        grid_areas: list[str],
    ) -> DataFrame:
        return T.get_metering_point_periods_df(
            self._table_reader,
            period_start_datetime,
            period_end_datetime,
            grid_areas,
        )

    @logging_configuration.use_span("get_grid_loss_responsible")
    def get_grid_loss_responsible(
        self, grid_areas: list[str], metering_point_periods_df: DataFrame
    ) -> GridLossResponsible:
        return T.get_grid_loss_responsible(
            grid_areas, metering_point_periods_df, self._table_reader
        )

    @logging_configuration.use_span("get_metering_point_time_series")
    def get_metering_point_time_series(
        self,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
        metering_point_periods_df: DataFrame,
    ) -> DataFrame:
        time_series_points_df = T.get_time_series_points(
            self._table_reader, period_start_datetime, period_end_datetime
        )
        return T.get_metering_point_time_series(
            time_series_points_df,
            metering_point_periods_df,
        )

    @logging_configuration.use_span("get_input_charges")
    def get_input_charges(
        self,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
    ) -> Tuple[ChargeMasterData, ChargePrices, DataFrame]:
        charge_master_data = T.read_charge_master_data(
            self._table_reader, period_start_datetime, period_end_datetime
        )

        charge_prices = T.read_charge_prices(
            self._table_reader, period_start_datetime, period_end_datetime
        )

        charge_links = T.read_charge_links(
            self._table_reader, period_start_datetime, period_end_datetime
        )

        return charge_master_data, charge_prices, charge_links

    def get_prepared_charges(
        self,
        metering_point_periods: DataFrame,
        time_series: DataFrame,
        charge_master_data: ChargeMasterData,
        charge_prices: ChargePrices,
        charge_links: DataFrame,
        time_zone: str,
    ) -> Tuple[PreparedTariffs, PreparedTariffs, DataFrame]:
        charge_link_metering_point_periods = T.get_charge_link_metering_point_periods(
            charge_links, metering_point_periods
        )

        prepared_tariffs_from_hourly = T.get_prepared_tariffs(
            time_series,
            charge_master_data,
            charge_prices,
            charge_link_metering_point_periods,
            ChargeResolution.HOUR,
            time_zone,
        )

        prepared_tariffs_from_daily = T.get_prepared_tariffs(
            time_series,
            charge_master_data,
            charge_prices,
            charge_link_metering_point_periods,
            ChargeResolution.DAY,
            time_zone,
        )

        prepared_subscription = T.get_subscription_charges(
            charge_master_data,
            charge_prices,
            charge_link_metering_point_periods,
            time_zone,
        )
        return (
            prepared_tariffs_from_hourly,
            prepared_tariffs_from_daily,
            prepared_subscription,
        )
