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

from pyspark.sql import DataFrame
from package.constants import Colname
from package.calculation_input import TableReader


def read_charges(table_reader: TableReader) -> DataFrame:
    charge_master_data_df = table_reader.read_charge_master_data_periods()
    charge_links_df = table_reader.read_charge_links_periods()
    charge_prices_df = table_reader.read_charge_price_points()

    return _create_charges_df(charge_master_data_df, charge_links_df, charge_prices_df)


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
        df[Colname.charge_id],
        df[Colname.charge_type],
        df[Colname.charge_owner],
        df[Colname.charge_tax],
        df[Colname.charge_resolution],
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
        df[Colname.charge_id],
        df[Colname.charge_type],
        df[Colname.charge_owner],
        df[Colname.charge_tax],
        df[Colname.charge_resolution],
        df[Colname.charge_time],
        df[Colname.from_date],
        df[Colname.to_date],
        df[Colname.charge_price],
        charge_links[Colname.metering_point_id],
    )
    return df
