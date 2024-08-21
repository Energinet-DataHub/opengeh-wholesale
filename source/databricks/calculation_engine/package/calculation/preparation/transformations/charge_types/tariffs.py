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
import pyspark.sql.functions as f
from pyspark.sql.dataframe import DataFrame
from pyspark.sql.types import DecimalType, StringType, ArrayType

import package.calculation.energy.aggregators.transformations as t
from package.calculation.preparation.data_structures.charge_link_metering_point_periods import (
    ChargeLinkMeteringPointPeriods,
)
from package.calculation.preparation.data_structures.charge_price_information import (
    ChargePriceInformation,
)
from package.calculation.preparation.data_structures.charge_prices import ChargePrices
from package.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from package.calculation.preparation.data_structures.prepared_tariffs import (
    PreparedTariffs,
)
from package.calculation.preparation.transformations.charge_types.explode_charge_price_information_within_periods import (
    explode_charge_price_information_within_periods,
)
from package.codelists import ChargeType, ChargeResolution
from package.constants import Colname


def get_prepared_tariffs(
    metering_point_time_series: PreparedMeteringPointTimeSeries,
    charge_price_information: ChargePriceInformation,
    charge_prices: ChargePrices,
    charge_link_metering_points: ChargeLinkMeteringPointPeriods,
    resolution: ChargeResolution,
    time_zone: str,
) -> PreparedTariffs:
    """
    metering_point_time_series always hava a row for each resolution time in the given period.
    """
    tariff_links = charge_link_metering_points.filter_by_charge_type(ChargeType.TARIFF)
    tariff_price_information_periods = charge_price_information.filter_by_charge_type(
        ChargeType.TARIFF
    )
    tariff_prices = charge_prices.filter_by_charge_type(ChargeType.TARIFF)

    tariffs = _join_price_information_periods_and_prices_add_missing_prices(
        tariff_price_information_periods, tariff_prices, resolution, time_zone
    )

    tariffs = _join_with_charge_link_metering_points(tariffs, tariff_links)

    # group by time series on metering point id and resolution and sum quantity
    grouped_time_series = (
        _group_by_time_series_on_metering_point_id_and_resolution_and_sum_quantity(
            metering_point_time_series, resolution, time_zone
        )
    )

    # join with grouped time series
    tariffs = _join_with_grouped_time_series(tariffs, grouped_time_series)

    return PreparedTariffs(tariffs)


def _join_price_information_periods_and_prices_add_missing_prices(
    charge_price_information: ChargePriceInformation,
    charge_prices: ChargePrices,
    resolution: ChargeResolution,
    time_zone: str,
) -> DataFrame:
    charge_prices = charge_prices.df

    charge_price_information = ChargePriceInformation(
        charge_price_information.df.filter(
            f.col(Colname.resolution) == resolution.value
        )
    )

    charge_price_information_with_charge_time = (
        explode_charge_price_information_within_periods(
            charge_price_information, resolution, time_zone
        )
    )

    charges_with_prices_and_missing_prices = (
        charge_price_information_with_charge_time.join(
            charge_prices, [Colname.charge_key, Colname.charge_time], "left"
        ).select(
            charge_price_information_with_charge_time[Colname.charge_key],
            charge_price_information_with_charge_time[Colname.charge_code],
            charge_price_information_with_charge_time[Colname.charge_type],
            charge_price_information_with_charge_time[Colname.charge_owner],
            charge_price_information_with_charge_time[Colname.charge_tax],
            charge_price_information_with_charge_time[Colname.resolution],
            charge_price_information_with_charge_time[Colname.charge_time],
            charge_price_information_with_charge_time[Colname.from_date],
            charge_price_information_with_charge_time[Colname.to_date],
            Colname.charge_price,
        )
    )

    return charges_with_prices_and_missing_prices


def _join_with_charge_link_metering_points(
    tariffs: DataFrame, charge_link_metering_points: ChargeLinkMeteringPointPeriods
) -> DataFrame:
    charge_link_metering_point_periods_df = charge_link_metering_points.df

    df = tariffs.join(
        charge_link_metering_point_periods_df,
        [
            tariffs[Colname.charge_key]
            == charge_link_metering_point_periods_df[Colname.charge_key],
            tariffs[Colname.charge_time]
            >= charge_link_metering_point_periods_df[Colname.from_date],
            tariffs[Colname.charge_time]
            < charge_link_metering_point_periods_df[Colname.to_date],
        ],
        "inner",
    ).select(
        tariffs[Colname.charge_key],
        tariffs[Colname.charge_code],
        tariffs[Colname.charge_type],
        tariffs[Colname.charge_owner],
        tariffs[Colname.charge_tax],
        tariffs[Colname.resolution],
        tariffs[Colname.charge_time],
        tariffs[Colname.charge_price],
        Colname.metering_point_id,
        charge_link_metering_point_periods_df[Colname.metering_point_type],
        charge_link_metering_point_periods_df[Colname.settlement_method],
        charge_link_metering_point_periods_df[Colname.grid_area_code],
        charge_link_metering_point_periods_df[Colname.energy_supplier_id],
    )
    return df


def _group_by_time_series_on_metering_point_id_and_resolution_and_sum_quantity(
    metering_point_time_series: PreparedMeteringPointTimeSeries,
    charge_resolution: ChargeResolution,
    time_zone: str,
) -> DataFrame:
    time_series_df = metering_point_time_series.df

    if charge_resolution == ChargeResolution.DAY:
        # Convert into local time zone to handle daylight saving time correctly
        time_series_df = time_series_df.withColumn(
            Colname.observation_time,
            f.from_utc_timestamp(Colname.observation_time, time_zone),
        )

    grouped_time_series = t.aggregate_quantity_and_quality(
        time_series_df,
        [
            Colname.metering_point_id,
            f.window(
                Colname.observation_time,
                _get_window_duration_string_based_on_resolution(charge_resolution),
            ).alias("time_window"),
        ],
    ).select(
        Colname.quantity,
        Colname.qualities,
        Colname.metering_point_id,
        f.col("time_window.start").alias(Colname.observation_time),
    )

    if charge_resolution == ChargeResolution.DAY:
        # Convert back to UTC
        grouped_time_series = grouped_time_series.withColumn(
            Colname.observation_time,
            f.to_utc_timestamp(Colname.observation_time, time_zone),
        )

    # The sum operator creates by default a column as a double type (28,6).
    # It must be cast to a decimal type (18,3) to conform to the tariff schema.
    # The column is renamed to quantity to match the column name from subscriptions and fees.
    grouped_time_series = grouped_time_series.withColumn(
        Colname.quantity, f.col(Colname.quantity).cast(DecimalType(18, 3))
    )

    grouped_time_series = grouped_time_series.withColumn(
        Colname.qualities, f.col(Colname.qualities).cast(ArrayType(StringType(), True))
    )

    return grouped_time_series


def _get_window_duration_string_based_on_resolution(
    resolution_duration: ChargeResolution,
) -> str:
    window_duration_string = "1 hour"

    if resolution_duration == ChargeResolution.DAY:
        window_duration_string = "1 day"

    return window_duration_string


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
        df[Colname.charge_code],
        df[Colname.charge_type],
        df[Colname.charge_owner],
        df[Colname.charge_tax],
        df[Colname.resolution],
        df[Colname.charge_time],
        df[Colname.charge_price],
        df[Colname.metering_point_id],
        df[Colname.energy_supplier_id],
        df[Colname.metering_point_type],
        df[Colname.settlement_method],
        df[Colname.grid_area_code],
        grouped_time_series[Colname.quantity],
        grouped_time_series[Colname.qualities],
    )
    return df
