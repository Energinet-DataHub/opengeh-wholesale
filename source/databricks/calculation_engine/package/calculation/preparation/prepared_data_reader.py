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
from pyspark.sql import DataFrame

from package.calculation_input import TableReader
from package.codelists import ChargeResolution
from package.calculation.preparation.grid_loss_responsible import GridLossResponsible

from . import transformations as T
from .charge_link_metering_point_periods import ChargeLinkMeteringPointPeriods
from .charge_master_data import ChargeMasterData
from .charge_prices import ChargePrices


class PreparedDataReader:
    def __init__(self, delta_table_reader: TableReader) -> None:
        self._table_reader = delta_table_reader

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

    def get_grid_loss_responsible(
        self, grid_areas: list[str], metering_point_periods_df: DataFrame
    ) -> GridLossResponsible:
        return T.get_grid_loss_responsible(
            grid_areas, metering_point_periods_df, self._table_reader
        )

    def get_charge_master_data(
        self,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
    ) -> ChargeMasterData:
        return T.read_charge_master_data(
            self._table_reader, period_start_datetime, period_end_datetime
        )

    def get_charge_prices(
        self,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
    ) -> ChargePrices:
        return T.read_charge_prices(
            self._table_reader, period_start_datetime, period_end_datetime
        )

    def get_charge_link_metering_point_periods(
        self,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
        metering_point_periods_df: DataFrame,
    ) -> ChargeLinkMeteringPointPeriods:
        charge_links = T.read_charge_links(
            self._table_reader, period_start_datetime, period_end_datetime
        )
        return T.get_charge_link_metering_point_periods(
            charge_links, metering_point_periods_df
        )

    def get_fee_charges(
        self,
        charge_master_data: ChargeMasterData,
        charge_prices: ChargePrices,
        charge_link_metering_points: ChargeLinkMeteringPointPeriods,
    ) -> DataFrame:
        return T.get_fee_charges(
            charge_master_data, charge_prices, charge_link_metering_points
        )

    def get_subscription_charges(
        self,
        charge_master_data: ChargeMasterData,
        charge_prices: ChargePrices,
        charge_link_metering_points: ChargeLinkMeteringPointPeriods,
        time_zone: str,
    ) -> DataFrame:
        return T.get_subscription_charges(
            charge_master_data, charge_prices, charge_link_metering_points, time_zone
        )

    def get_tariff_charges(
        self,
        time_series: DataFrame,
        charge_master_data: ChargeMasterData,
        charge_prices: ChargePrices,
        charges_link_metering_point_periods: ChargeLinkMeteringPointPeriods,
        resolution: ChargeResolution,
        time_zone: str,
    ) -> DataFrame:
        return T.get_tariff_charges(
            time_series,
            charge_master_data,
            charge_prices,
            charges_link_metering_point_periods,
            resolution,
            time_zone,
        )

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
