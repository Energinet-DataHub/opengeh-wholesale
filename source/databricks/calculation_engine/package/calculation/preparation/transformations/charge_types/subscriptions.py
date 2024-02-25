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
from pyspark.sql import Window
from pyspark.sql.dataframe import DataFrame

from package.calculation.preparation.charge_link_metering_point_periods import (
    ChargeLinkMeteringPointPeriods,
)
from package.codelists import ChargeType
from package.constants import Colname


def get_subscription_charges(
    charge_master_data: DataFrame,
    charge_prices: DataFrame,
    charge_link_metering_point_periods: ChargeLinkMeteringPointPeriods,
    time_zone: str,
) -> DataFrame:
    subscription_links = _filter_subscriptions(charge_link_metering_point_periods.df)
    subscription_prices = _filter_subscriptions(charge_prices)
    subscription_master_data = _filter_subscriptions(charge_master_data)

    subscription_master_data_and_prices = _join_with_prices(
        subscription_master_data, subscription_prices, time_zone
    )

    subscriptions = _join_with_links(
        subscription_master_data_and_prices, subscription_links
    )

    return subscriptions


def _filter_subscriptions(df: DataFrame) -> DataFrame:
    return df.filter(f.col(Colname.charge_type) == ChargeType.SUBSCRIPTION.value)


def _join_with_prices(
    subscription_master_data: DataFrame,
    subscription_prices: DataFrame,
    time_zone: str,
) -> DataFrame:
    """
    Join subscription_master_data with subscription_prices.
    This method also ensure
    - Missing charge prices will be set to None.
    - The charge price is the last known charge price for the charge key.
    """
    subscription_master_data_with_charge_time = _expand_with_daily_charge_time(
        subscription_master_data, time_zone
    )

    w = Window.partitionBy(Colname.charge_key, Colname.from_date).orderBy(
        Colname.charge_time
    )

    result = subscription_master_data_with_charge_time.join(
        subscription_prices, [Colname.charge_key, Colname.charge_time], "left"
    ).withColumn(
        Colname.charge_price,
        f.last(Colname.charge_price, ignorenulls=True).over(w),
    )

    return result


def _expand_with_daily_charge_time(
    subscription_master_data: DataFrame, time_zone: str
) -> DataFrame:
    """
    Add charge_time column to subscription_periods DataFrame.
    The charge_time column is created by exploding subscription_periods using from_date and to_date with a resolution of 1 day.
    """

    charge_time_local = "charge_time_local"

    charge_periods_with_charge_time = (
        subscription_master_data.withColumn(
            charge_time_local,
            f.explode(
                f.expr(
                    f"sequence(from_utc_timestamp({Colname.from_date}, '{time_zone}'), from_utc_timestamp({Colname.to_date}, '{time_zone}'), interval 1 day)"
                )
            ).alias("charge_time_local"),
        )
        .withColumn(
            Colname.charge_time,
            f.to_utc_timestamp(f.col(charge_time_local), time_zone),
        )
        .drop(charge_time_local)
    )

    return charge_periods_with_charge_time


def _join_with_links(
    subscription_period_prices: DataFrame, subscription_links: DataFrame
) -> DataFrame:
    subscriptions = subscription_period_prices.join(
        subscription_links,
        (
            subscription_period_prices[Colname.charge_key]
            == subscription_links[Colname.charge_key]
        )
        & (
            subscription_period_prices[Colname.charge_time]
            >= subscription_links[Colname.from_date]
        )
        & (
            subscription_period_prices[Colname.charge_time]
            < subscription_links[Colname.to_date]
        ),
        how="inner",
    ).select(
        subscription_period_prices[Colname.charge_key],
        Colname.charge_time,
        Colname.charge_price,
        subscription_links[Colname.metering_point_type],
        subscription_links[Colname.settlement_method],
        subscription_links[Colname.grid_area],
        subscription_links[Colname.energy_supplier_id],
    )

    return subscriptions
