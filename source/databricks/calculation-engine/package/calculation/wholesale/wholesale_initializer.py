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
from pyspark.sql.functions import (
    col,
    collect_set,
    window,
    expr,
    explode,
    month,
    year,
    sum,
)
from pyspark.sql.types import DecimalType, StringType, ArrayType

from package.codelists import ChargeType, ChargeResolution
from package.constants import Colname


def get_tariff_charges(
    metering_points: DataFrame,
    time_series: DataFrame,
    charge_master_data: DataFrame,
    charge_links: DataFrame,
    charge_prices: DataFrame,
    resolution_duration: ChargeResolution,
) -> DataFrame:
    # filter on resolution
    charge_master_data = _get_charges_based_on_resolution(
        charge_master_data, resolution_duration
    )

    df = _join_properties_on_charges_with_given_charge_type(
        charge_master_data,
        charge_prices,
        charge_links,
        metering_points,
        ChargeType.TARIFF,
    )

    # group by time series on metering point id and resolution and sum quantity
    grouped_time_series = (
        _group_by_time_series_on_metering_point_id_and_resolution_and_sum_quantity(
            time_series, resolution_duration
        )
    )

    # join with grouped time series
    df = _join_with_grouped_time_series(df, grouped_time_series)

    # When constructing the tariff dataframe some column types need to be not nullable (or nullable)
    # The construct should make it impossible for them to be null (or not null).
    # df.schema[Colname.qualities].nullable = True
    df.schema[Colname.quantity].nullable = False
    df.schema[Colname.metering_point_type].nullable = False
    df.schema[Colname.energy_supplier_id].nullable = False
    return df


def get_fee_charges(
    charge_master_data: DataFrame,
    charge_prices: DataFrame,
    charge_links: DataFrame,
    metering_points: DataFrame,
) -> DataFrame:
    return _join_properties_on_charges_with_given_charge_type(
        charge_master_data,
        charge_prices,
        charge_links,
        metering_points,
        ChargeType.FEE,
    )


def get_subscription_charges(
    charge_master_data: DataFrame,
    charge_prices: DataFrame,
    charge_links: DataFrame,
    metering_points: DataFrame,
) -> DataFrame:
    return _join_properties_on_charges_with_given_charge_type(
        charge_master_data,
        charge_prices,
        charge_links,
        metering_points,
        ChargeType.SUBSCRIPTION,
    )


def _get_charges_based_on_resolution(
    charge_master_data: DataFrame, resolution_duration: ChargeResolution
) -> DataFrame:
    df = charge_master_data.filter(
        col(Colname.charge_resolution) == resolution_duration.value
    )
    return df


def _get_charges_based_on_charge_type(
    charge_master_data: DataFrame, charge_type: ChargeType
) -> DataFrame:
    df = charge_master_data.filter(col(Colname.charge_type) == charge_type.value)
    return df


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


def _explode_subscription(charges_with_prices: DataFrame) -> DataFrame:
    charges_with_prices = (
        charges_with_prices.withColumn(
            Colname.date,
            explode(
                expr(
                    f"sequence({Colname.from_date}, {Colname.to_date}, interval 1 day)"
                )
            ),
        )
        .filter((year(Colname.date) == year(Colname.charge_time)))
        .filter((month(Colname.date) == month(Colname.charge_time)))
        .select(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.charge_tax,
            Colname.charge_resolution,
            Colname.date,
            Colname.charge_price,
        )
        .withColumnRenamed(Colname.date, Colname.charge_time)
    )
    return charges_with_prices


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
        df[Colname.charge_price],
        charge_links[Colname.metering_point_id],
    )
    return df


def _join_with_metering_points(df: DataFrame, metering_points: DataFrame) -> DataFrame:
    df = df.join(
        metering_points,
        [
            df[Colname.metering_point_id] == metering_points[Colname.metering_point_id],
            df[Colname.charge_time] >= metering_points[Colname.from_date],
            df[Colname.charge_time] < metering_points[Colname.to_date],
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
        df[Colname.charge_price],
        df[Colname.metering_point_id],
        metering_points[Colname.metering_point_type],
        metering_points[Colname.settlement_method],
        metering_points[Colname.grid_area],
        metering_points[Colname.energy_supplier_id],
    )
    return df


def _group_by_time_series_on_metering_point_id_and_resolution_and_sum_quantity(
    time_series: DataFrame, resolution_duration: ChargeResolution
) -> DataFrame:
    grouped_time_series = (
        time_series.groupBy(
            Colname.metering_point_id,
            window(
                Colname.observation_time,
                _get_window_duration_string_based_on_resolution(resolution_duration),
            ),
        )
        .agg(
            sum(Colname.quantity).alias(Colname.quantity),
            collect_set(Colname.quality).alias(Colname.qualities),
        )
        .withColumnRenamed(f"sum({Colname.quantity})", Colname.quantity)
        .selectExpr(
            Colname.quantity,
            Colname.qualities,
            Colname.metering_point_id,
            f"window.{Colname.start} as {Colname.charge_time}",
        )
    )

    # The sum operator creates by default a column as a double type (28,6).
    # It must be cast to a decimal type (18,3) to conform to the tariff schema.
    grouped_time_series = grouped_time_series.withColumn(
        Colname.quantity, col(Colname.quantity).cast(DecimalType(18, 3))
    )

    grouped_time_series = grouped_time_series.withColumn(
        Colname.qualities, col(Colname.qualities).cast(ArrayType(StringType(), True))
    )

    return grouped_time_series


def _join_with_grouped_time_series(
    df: DataFrame, grouped_time_series: DataFrame
) -> DataFrame:
    df = df.join(
        grouped_time_series,
        [
            df[Colname.metering_point_id]
            == grouped_time_series[Colname.metering_point_id],
            df[Colname.charge_time] == grouped_time_series[Colname.observation_time],
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
        df[Colname.charge_price],
        df[Colname.metering_point_id],
        df[Colname.energy_supplier_id],
        df[Colname.metering_point_type],
        df[Colname.settlement_method],
        df[Colname.grid_area],
        grouped_time_series[Colname.quantity],
        grouped_time_series[Colname.qualities],
    )
    return df


def _get_window_duration_string_based_on_resolution(
    resolution_duration: ChargeResolution,
) -> str:
    window_duration_string = "1 hour"

    if resolution_duration == ChargeResolution.DAY:
        window_duration_string = "1 day"

    if resolution_duration == ChargeResolution.MONTH:
        raise NotImplementedError("Month not yet implemented")

    return window_duration_string


# Join charge_master_data, charge prices, charge links, and metering points together. On given charge type
def _join_properties_on_charges_with_given_charge_type(
    charge_master_data: DataFrame,
    charge_prices: DataFrame,
    charge_links: DataFrame,
    metering_points: DataFrame,
    charge_type: ChargeType,
) -> DataFrame:
    # filter on charge_type
    charge_master_data = _get_charges_based_on_charge_type(
        charge_master_data, charge_type
    )

    # join charge prices with charge_master_data
    charges_with_prices = _join_with_charge_prices(charge_master_data, charge_prices)

    if charge_type == ChargeType.SUBSCRIPTION:
        # Explode dataframe: create row for each day the time period from and to date
        charges_with_prices = _explode_subscription(charges_with_prices)

    # join charge links with charges_with_prices
    charges_with_price_and_links = _join_with_charge_links(
        charges_with_prices, charge_links
    )

    df = _join_with_metering_points(charges_with_price_and_links, metering_points)

    if charge_type != ChargeType.TARIFF:
        df = df.select(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.charge_time,
            Colname.charge_price,
            Colname.metering_point_type,
            Colname.settlement_method,
            Colname.grid_area,
            Colname.energy_supplier_id,
        )

    return df
