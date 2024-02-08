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

from package.calculation.preparation.transformations.clamp_period import clamp_period
from pyspark.sql.functions import col, concat_ws

from package.constants import Colname
from package.calculation_input import TableReader


def read_charges(
    table_reader: TableReader,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:
    charge_prices_df = _get_charge_price_points(
        table_reader, period_start_datetime, period_end_datetime
    )
    charge_master_data_df = _get_charge_master_data(
        table_reader, period_start_datetime, period_end_datetime
    )
    charge_links_df = _get_charge_links(
        table_reader, period_start_datetime, period_end_datetime
    )
    charge_prices_df.show()
    charge_master_data_df.show()
    charge_links_df.show()
    return _create_charges_df(charge_master_data_df, charge_links_df, charge_prices_df)


def _get_charge_master_data(
    table_reader: TableReader,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:
    charge_master_data_df = (
        table_reader.read_charge_master_data_periods()
        .where(col(Colname.from_date) < period_end_datetime)
        .where(
            col(Colname.to_date).isNull()
            | (col(Colname.to_date) > period_start_datetime)
        )
    )
    charge_master_data_df = clamp_period(
        charge_master_data_df,
        period_start_datetime,
        period_end_datetime,
        Colname.from_date,
        Colname.to_date,
    )
    charge_master_data_df = _add_charge_key_column(charge_master_data_df)
    return charge_master_data_df


def _get_charge_links(
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


def _get_charge_price_points(
    table_reader: TableReader,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:
    charge_price_points_df = (
        table_reader.read_charge_price_points()
        .where(col(Colname.charge_time) >= period_start_datetime)
        .where(col(Colname.charge_time) < period_end_datetime)
    )

    charge_price_points_df = _add_charge_key_column(charge_price_points_df)
    return charge_price_points_df


def _create_charges_df(
    charge_master_data: DataFrame,
    charge_links: DataFrame,
    charge_prices: DataFrame,
) -> DataFrame:
    charges_with_prices = _join_with_charge_prices(charge_master_data, charge_prices)
    charges_with_price_and_links = _join_with_charge_links(
        charges_with_prices, charge_links
    )
    return charges_with_price_and_links


def _join_with_charge_prices(df: DataFrame, charge_prices: DataFrame) -> DataFrame:
    df = df.join(charge_prices, [Colname.charge_key], "inner").select(
        df[Colname.charge_key],
        df[Colname.charge_code],
        df[Colname.charge_type],
        df[Colname.charge_owner],
        df[Colname.charge_tax],
        df[Colname.resolution],
        df[Colname.from_date],
        df[Colname.to_date],
        charge_prices[Colname.charge_time],
        charge_prices[Colname.charge_price],
    )
    return df


def _join_with_charge_links(df: DataFrame, charge_links: DataFrame) -> DataFrame:
    df = df.join(
        charge_links,
        [
            df[Colname.charge_key] == charge_links[Colname.charge_key],
            df[Colname.charge_time] >= charge_links[Colname.from_date],
            df[Colname.charge_time] < charge_links[Colname.to_date],
        ],
        "inner",
    ).select(
        df[Colname.charge_key],
        df[Colname.charge_code],
        df[Colname.charge_type],
        df[Colname.charge_owner],
        df[Colname.charge_tax],
        df[Colname.resolution],
        df[Colname.charge_time],
        df[Colname.charge_price],
        charge_links[Colname.from_date],
        charge_links[Colname.to_date],
        charge_links[Colname.metering_point_id],
    )
    return df


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
