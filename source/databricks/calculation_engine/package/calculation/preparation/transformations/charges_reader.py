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
from pyspark.sql.functions import col, concat_ws

from package.calculation.preparation.charge_master_data import ChargeMasterData
from package.calculation.preparation.charge_prices import ChargePrices
from package.calculation.preparation.transformations.clamp_period import clamp_period
from package.calculation_input import TableReader
from package.constants import Colname


def read_charge_master_data(
    table_reader: TableReader,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> ChargeMasterData:
    return _get_charge_master_data_periods(
        table_reader, period_start_datetime, period_end_datetime
    )


def read_charge_prices(
    table_reader: TableReader,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> ChargePrices:
    return _get_charge_price_points(
        table_reader, period_start_datetime, period_end_datetime
    )


def read_charge_links(
    table_reader: TableReader,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:
    charge_links_df = (
        table_reader.read_charge_links_periods()
        .where(col(Colname.from_date) < period_end_datetime)
        .where(
            col(Colname.to_date).isNull()
            | (col(Colname.to_date) > period_start_datetime)
        )
    )

    charge_links_df = clamp_period(
        charge_links_df,
        period_start_datetime,
        period_end_datetime,
        Colname.from_date,
        Colname.to_date,
    )
    charge_links_df = _add_charge_key_column(charge_links_df)

    return charge_links_df


def _get_charge_master_data_periods(
    table_reader: TableReader,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> ChargeMasterData:
    charge_master_data_periods = (
        table_reader.read_charge_master_data_periods()
        .where(col(Colname.from_date) < period_end_datetime)
        .where(
            col(Colname.to_date).isNull()
            | (col(Colname.to_date) > period_start_datetime)
        )
    )

    charge_master_data_periods = clamp_period(
        charge_master_data_periods,
        period_start_datetime,
        period_end_datetime,
        Colname.from_date,
        Colname.to_date,
    )

    charge_master_data_periods = _add_charge_key_column(charge_master_data_periods)
    return ChargeMasterData(charge_master_data_periods)


def _get_charge_price_points(
    table_reader: TableReader,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> ChargePrices:
    charge_price_points = (
        table_reader.read_charge_price_points()
        .where(col(Colname.charge_time) >= period_start_datetime)
        .where(col(Colname.charge_time) < period_end_datetime)
    )

    charge_price_points = _add_charge_key_column(charge_price_points)
    return ChargePrices(charge_price_points)


def _add_charge_key_column(charge_df: DataFrame) -> DataFrame:
    return charge_df.withColumn(
        Colname.charge_key,
        concat_ws(
            "-",
            col(Colname.charge_code),
            col(Colname.charge_owner),
            col(Colname.charge_type),
        ),
    )
