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
import pyspark.sql.functions as F
from pyspark.sql.types import DecimalType, StringType, ArrayType

from package.codelists import ChargeType, ChargeResolution
from package.constants import Colname


def get_tariff_charges(
    metering_points: DataFrame,
    time_series: DataFrame,
    charges_df: DataFrame,
    resolution_duration: ChargeResolution,
) -> DataFrame:
    # filter on resolution
    charges_df = _get_charges_based_on_resolution(charges_df, resolution_duration)

    df = _join_properties_on_charges_with_given_charge_type(
        charges_df,
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

    # When constructing the tariff dataframe some column types need to be not nullable
    # (or nullable).
    # The construct should make it impossible for them to be null (or not null).
    # df.schema[Colname.qualities].nullable = True
    df.schema[Colname.quantity].nullable = False
    df.schema[Colname.metering_point_type].nullable = False
    df.schema[Colname.energy_supplier_id].nullable = False
    return df


def get_fee_charges(
    charges_df: DataFrame,
    metering_points: DataFrame,
) -> DataFrame:
    return _join_properties_on_charges_with_given_charge_type(
        charges_df,
        metering_points,
        ChargeType.FEE,
    )


def get_subscription_charges(
    charges_df: DataFrame,
    metering_points: DataFrame,
) -> DataFrame:
    return _join_properties_on_charges_with_given_charge_type(
        charges_df,
        metering_points,
        ChargeType.SUBSCRIPTION,
    )


def _get_charges_based_on_resolution(
    charges_df: DataFrame, resolution_duration: ChargeResolution
) -> DataFrame:
    return charges_df.filter(
        F.col(Colname.charge_resolution) == resolution_duration.value
    )


def _get_charges_based_on_charge_type(
    charges_df: DataFrame, charge_type: ChargeType
) -> DataFrame:
    return charges_df.filter(F.col(Colname.charge_type) == charge_type.value)


def _explode_subscription(charges_df: DataFrame) -> DataFrame:
    charges_df = (
        charges_df.withColumn(
            Colname.date,
            F.explode(
                F.expr(
                    f"sequence({Colname.from_date}, {Colname.to_date}, interval 1 day)"
                )
            ),
        )
        .filter((F.year(Colname.date) == F.year(Colname.charge_time)))
        .filter((F.month(Colname.date) == F.month(Colname.charge_time)))
        .drop(Colname.charge_time)
        .withColumnRenamed(Colname.date, Colname.charge_time)
        .select(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.charge_tax,
            Colname.charge_resolution,
            Colname.charge_time,
            Colname.charge_price,
            Colname.metering_point_id,
        )
    )
    return charges_df


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
            F.window(
                Colname.observation_time,
                _get_window_duration_string_based_on_resolution(resolution_duration),
            ),
        )
        .agg(
            F.sum(Colname.quantity).alias(Colname.quantity),
            F.collect_set(Colname.quality).alias(Colname.qualities),
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
        Colname.quantity, F.col(Colname.quantity).cast(DecimalType(18, 3))
    )

    grouped_time_series = grouped_time_series.withColumn(
        Colname.qualities, F.col(Colname.qualities).cast(ArrayType(StringType(), True))
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


# Join charge_master_data, charge prices, charge links, and metering points together.
# On given charge type.
def _join_properties_on_charges_with_given_charge_type(
    charges_df: DataFrame,
    metering_points: DataFrame,
    charge_type: ChargeType,
) -> DataFrame:
    # filter on charge_type
    charges_df = _get_charges_based_on_charge_type(charges_df, charge_type)

    if charge_type == ChargeType.SUBSCRIPTION:
        # Explode dataframe: create row for each day the time period from and to date
        charges_df = _explode_subscription(charges_df)

    df = _join_with_metering_points(charges_df, metering_points)

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
