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

from . import transformations as t


class PreparedDataReader:
    def __init__(self, delta_table_reader: TableReader) -> None:
        self._table_reader = delta_table_reader

    def get_metering_point_periods_df(
        self,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
        grid_areas: list[str],
    ) -> DataFrame:
        return t.get_metering_point_periods_df(
            self._table_reader,
            period_start_datetime,
            period_end_datetime,
            grid_areas,
        )

    def get_grid_loss_responsible(
        self, grid_areas: list[str],
        metering_point_periods_df: DataFrame
    ) -> GridLossResponsible:
        return t.get_grid_loss_responsible(
            grid_areas, metering_point_periods_df, self._table_reader
        )

    def get_charges(
        self,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
    ) -> DataFrame:
        return t.read_charges(
            self._table_reader, period_start_datetime, period_end_datetime
        )

    def get_fee_charges(
        self,
        charges_df: DataFrame,
        metering_points: DataFrame,
    ) -> DataFrame:
        return t.get_fee_charges(charges_df, metering_points)

    def get_subscription_charges(
        self,
        charges_df: DataFrame,
        metering_points: DataFrame,
    ) -> DataFrame:
        return t.get_subscription_charges(charges_df, metering_points)

    def get_tariff_charges(
        self,
        metering_points: DataFrame,
        time_series: DataFrame,
        charges_df: DataFrame,
        resolution: ChargeResolution,
    ) -> DataFrame:
        return t.get_tariff_charges(
            metering_points, time_series, charges_df, resolution
        )

    def get_metering_point_time_series(
        self,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
        metering_point_periods_df: DataFrame,
    ) -> DataFrame:
        time_series_points_df = t.get_time_series_points(
            self._table_reader, period_start_datetime, period_end_datetime
        )
        return t.get_metering_point_time_series(
            time_series_points_df,
            metering_point_periods_df,
        )
