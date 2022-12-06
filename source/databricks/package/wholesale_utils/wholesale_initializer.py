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

from pyspark.sql.dataframe import DataFrame
from pyspark.sql.functions import col, window, expr, explode, month, year
from geh_stream.codelists import Colname, ResolutionDuration, ConnectionState, ChargeType


charge_from_date = "charge_from_date"
charge_to_date = "charge_to_date"
charge_link_from_date = "charge_link_from_date"
charge_link_to_date = "charge_link_to_date"
market_roles_from_date = "market_roles_from_date"
market_roles_to_date = "market_roles_to_date"
metering_point_from_date = "metering_point_from_date"
metering_point_to_date = "metering_point_to_date"


def get_tariff_charges(
        time_series: DataFrame,
        charges: DataFrame,
        charge_links: DataFrame,
        charge_prices: DataFrame,
        metering_points: DataFrame,
        market_roles: DataFrame,
        resolution_duration: ResolutionDuration
        ) -> DataFrame:

    # filter on resolution
    charges = get_charges_based_on_resolution(charges, resolution_duration)

    df = __join_properties_on_charges_with_given_charge_type(charges, charge_prices, charge_links, metering_points, market_roles, ChargeType.tariff)

    # group by time series on metering point id and resolution and sum quantity
    grouped_time_series = group_by_time_series_on_metering_point_id_and_resolution_and_sum_quantity(time_series, resolution_duration)

    # join with grouped time series
    df = join_with_grouped_time_series(df, grouped_time_series)

    return df


def get_fee_charges(charges: DataFrame, charge_prices: DataFrame, charge_links: DataFrame, metering_points: DataFrame, market_roles: DataFrame) -> DataFrame:
    return __join_properties_on_charges_with_given_charge_type(charges, charge_prices, charge_links, metering_points, market_roles, ChargeType.fee)


def get_subscription_charges(charges: DataFrame, charge_prices: DataFrame, charge_links: DataFrame, metering_points: DataFrame, market_roles: DataFrame) -> DataFrame:
    return __join_properties_on_charges_with_given_charge_type(charges, charge_prices, charge_links, metering_points, market_roles, ChargeType.subscription)


def get_charges_based_on_resolution(charges: DataFrame, resolution_duration: ResolutionDuration) -> DataFrame:
    df = charges.filter(col(Colname.resolution) == resolution_duration)
    return df


def get_charges_based_on_charge_type(charges: DataFrame, charge_type: ChargeType) -> DataFrame:
    df = charges.filter(col(Colname.charge_type) == charge_type)
    return df


def join_with_charge_prices(df: DataFrame, charge_prices: DataFrame) -> DataFrame:
    df = df \
        .join(charge_prices, [Colname.charge_key], "inner") \
        .select(
            df[Colname.charge_key],
            df[Colname.charge_id],
            df[Colname.charge_type],
            df[Colname.charge_owner],
            df[Colname.charge_tax],
            df[Colname.resolution],
            df[Colname.from_date],
            df[Colname.to_date],
            charge_prices[Colname.time],
            charge_prices[Colname.charge_price]
        )
    return df


def explode_subscription(charges_with_prices: DataFrame) -> DataFrame:
    charges_with_prices = charges_with_prices.withColumn(Colname.date, explode(expr(f"sequence({Colname.from_date}, {Colname.to_date}, interval 1 day)"))) \
        .filter((year(Colname.date) == year(Colname.time))) \
        .filter((month(Colname.date) == month(Colname.time))) \
        .select(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.charge_tax,
            Colname.resolution,
            Colname.date,
            Colname.charge_price
        ).withColumnRenamed(Colname.date, Colname.time)
    return charges_with_prices


def join_with_charge_links(df: DataFrame, charge_links: DataFrame) -> DataFrame:
    df = df \
        .join(
        charge_links,
        [
            df[Colname.charge_key] == charge_links[Colname.charge_key],
            df[Colname.time] >= charge_links[Colname.from_date],
            df[Colname.time] < charge_links[Colname.to_date]
        ], "inner") \
        .select(
            df[Colname.charge_key],
            df[Colname.charge_id],
            df[Colname.charge_type],
            df[Colname.charge_owner],
            df[Colname.charge_tax],
            df[Colname.resolution],
            df[Colname.time],
            df[Colname.charge_price],
            charge_links[Colname.metering_point_id]
        )
    return df


def join_with_martket_roles(df: DataFrame, market_roles: DataFrame) -> DataFrame:
    df = df.join(
        market_roles,
        [
            df[Colname.metering_point_id] == market_roles[Colname.metering_point_id],
            df[Colname.time] >= market_roles[Colname.from_date],
            df[Colname.time] < market_roles[Colname.to_date]
        ], "inner") \
        .select(
            df[Colname.charge_key],
            df[Colname.charge_id],
            df[Colname.charge_type],
            df[Colname.charge_owner],
            df[Colname.charge_tax],
            df[Colname.resolution],
            df[Colname.time],
            df[Colname.charge_price],
            df[Colname.metering_point_id],
            market_roles[Colname.energy_supplier_id]
        )
    return df


def get_connected_metering_points(metering_points: DataFrame) -> DataFrame:
    metering_points = metering_points.filter(col(Colname.connection_state) == ConnectionState.connected.value)
    return metering_points


def join_with_metering_points(df: DataFrame, metering_points: DataFrame) -> DataFrame:
    df = df.join(
        metering_points,
        [
            df[Colname.metering_point_id] == metering_points[Colname.metering_point_id],
            df[Colname.time] >= metering_points[Colname.from_date],
            df[Colname.time] < metering_points[Colname.to_date]
        ], "inner") \
        .select(
            df[Colname.charge_key],
            df[Colname.charge_id],
            df[Colname.charge_type],
            df[Colname.charge_owner],
            df[Colname.charge_tax],
            df[Colname.resolution],
            df[Colname.time],
            df[Colname.charge_price],
            df[Colname.metering_point_id],
            df[Colname.energy_supplier_id],
            metering_points[Colname.metering_point_type],
            metering_points[Colname.connection_state],
            metering_points[Colname.settlement_method],
            metering_points[Colname.grid_area]
        )
    return df


def group_by_time_series_on_metering_point_id_and_resolution_and_sum_quantity(time_series: DataFrame, resolution_duration: ResolutionDuration) -> DataFrame:
    grouped_time_series = time_series \
        .groupBy(
            Colname.metering_point_id,
            window(Colname.time, __get_window_duration_string_based_on_resolution(resolution_duration))
        ) \
        .sum(Colname.quantity) \
        .withColumnRenamed(f'sum({Colname.quantity})', Colname.quantity) \
        .selectExpr(
            Colname.quantity,
            Colname.metering_point_id,
            f'window.{Colname.start} as {Colname.time}'
        )
    return grouped_time_series


def join_with_grouped_time_series(df: DataFrame, grouped_time_series: DataFrame) -> DataFrame:
    df = df.join(
        grouped_time_series,
        [
            df[Colname.metering_point_id] == grouped_time_series[Colname.metering_point_id],
            df[Colname.time] == grouped_time_series[Colname.time]
        ], "inner") \
        .select(
            df[Colname.charge_key],
            df[Colname.charge_id],
            df[Colname.charge_type],
            df[Colname.charge_owner],
            df[Colname.charge_tax],
            df[Colname.resolution],
            df[Colname.time],
            df[Colname.charge_price],
            df[Colname.metering_point_id],
            df[Colname.energy_supplier_id],
            df[Colname.metering_point_type],
            df[Colname.connection_state],
            df[Colname.settlement_method],
            df[Colname.grid_area],
            grouped_time_series[Colname.quantity]
        )
    return df


def __get_window_duration_string_based_on_resolution(resolution_duration: ResolutionDuration) -> str:
    window_duration_string = '1 hour'

    if(resolution_duration == ResolutionDuration.day):
        window_duration_string = '1 day'

    return window_duration_string


# Join charges, charge prices, charge links, metering points and market roles together. On given charge type
def __join_properties_on_charges_with_given_charge_type(charges: DataFrame, charge_prices: DataFrame, charge_links: DataFrame,
                                                        metering_points: DataFrame, market_roles: DataFrame,
                                                        charge_type: ChargeType) -> DataFrame:
    # filter on charge_type
    charges = get_charges_based_on_charge_type(charges, charge_type)

    # join charge prices with charges
    charges_with_prices = join_with_charge_prices(charges, charge_prices)

    if charge_type == ChargeType.subscription:
        # Explode dataframe: create row for each day the time period from and to date
        charges_with_prices = explode_subscription(charges_with_prices)

    # join charge links with charges_with_prices
    charges_with_price_and_links = join_with_charge_links(charges_with_prices, charge_links)

    df = join_with_martket_roles(charges_with_price_and_links, market_roles)

    metering_points = get_connected_metering_points(metering_points)

    df = join_with_metering_points(df, metering_points)

    if charge_type != ChargeType.tariff:
        df = df.select(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.time,
            Colname.charge_price,
            Colname.metering_point_type,
            Colname.settlement_method,
            Colname.grid_area,
            Colname.connection_state,
            Colname.energy_supplier_id
        )

    return df
