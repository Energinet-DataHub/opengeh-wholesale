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
from package.codelists import ChargeType, SettlementMethod, MeteringPointType
from package.calculation.preparation.charge_master_data import ChargeMasterData
from package.calculation.preparation.charge_prices import ChargePrices
from package.codelists import ChargeType
from package.constants import Colname


def get_subscription_charges(
    charge_master_data: ChargeMasterData,
    charge_prices: ChargePrices,
    charge_link_metering_point_periods: ChargeLinkMeteringPointPeriods,
    time_zone: str,
) -> DataFrame:
    subscription_links = charge_link_metering_point_periods.filter_by_charge_type(
        ChargeType.SUBSCRIPTION
    )
    subscription_links = _filter_on_flex_consumption(subscription_links.df)
    subscription_prices = charge_prices.filter_by_charge_type(ChargeType.SUBSCRIPTION)
    subscription_master_data = charge_master_data.filter_by_charge_type(
        ChargeType.SUBSCRIPTION
    )

    subscription_master_data_and_prices = _join_with_prices(
        subscription_master_data, subscription_prices, time_zone
    )

    subscriptions = _join_with_links(
        subscription_master_data_and_prices, subscription_links
    )

    return subscriptions


def _filter_on_flex_consumption(
    subscription_links: DataFrame,
) -> DataFrame:
    subscription_links = subscription_links.filter(
        f.col(Colname.metering_point_type) == MeteringPointType.CONSUMPTION.value
    ).filter(f.col(Colname.settlement_method) == SettlementMethod.FLEX.value)
    return subscription_links


def _join_with_prices(
    subscription_master_data: ChargeMasterData,
    subscription_prices: ChargePrices,
    time_zone: str,
) -> DataFrame:
    """
    Join subscription_master_data with subscription_prices.
    This method also ensure
    - Missing charge prices will be set to None.
    - The charge price is the last known charge price for the charge key.
    """
    subscription_prices = subscription_prices.df
    subscription_master_data = subscription_master_data.df

    subscription_master_data_with_charge_time = _expand_with_daily_charge_time(
        subscription_master_data, time_zone
    )

    w = Window.partitionBy(Colname.charge_key, Colname.from_date).orderBy(
        Colname.charge_time
    )

    master_data_with_prices = (
        subscription_master_data_with_charge_time.join(
            subscription_prices, [Colname.charge_key, Colname.charge_time], "left"
        )
        .withColumn(
            Colname.charge_price,
            f.last(Colname.charge_price, ignorenulls=True).over(w),
        )
        .select(
            subscription_master_data_with_charge_time[Colname.charge_key],
            subscription_master_data_with_charge_time[Colname.charge_type],
            subscription_master_data_with_charge_time[Colname.charge_owner],
            subscription_master_data_with_charge_time[Colname.charge_code],
            subscription_master_data_with_charge_time[Colname.from_date],
            subscription_master_data_with_charge_time[Colname.to_date],
            subscription_master_data_with_charge_time[Colname.resolution],
            subscription_master_data_with_charge_time[Colname.charge_tax],
            subscription_master_data_with_charge_time[Colname.charge_time],
            Colname.charge_price,
        )
    )

    return master_data_with_prices


def _expand_with_daily_charge_time(
    subscription_master_data: DataFrame, time_zone: str
) -> DataFrame:
    """
    Add charge_time column to subscription_periods DataFrame.
    The charge_time column is created by exploding subscription_periods using from_date and to_date with a resolution of 1 day.
    """

    charge_periods_with_charge_time = subscription_master_data.withColumn(
        Colname.charge_time,
        f.explode(
            f.sequence(
                f.from_utc_timestamp(Colname.from_date, time_zone),
                f.from_utc_timestamp(Colname.to_date, time_zone),
                f.expr("interval 1 day"),
            )
        ),
    ).withColumn(
        Colname.charge_time,
        f.to_utc_timestamp(Colname.charge_time, time_zone),
    )

    return charge_periods_with_charge_time


def _join_with_links(
    subscription_master_data_and_prices: DataFrame,
    subscription_links: DataFrame,
) -> DataFrame:
    subscriptions = subscription_master_data_and_prices.join(
        subscription_links,
        (
            subscription_master_data_and_prices[Colname.charge_key]
            == subscription_links[Colname.charge_key]
        )
        & (
            subscription_master_data_and_prices[Colname.charge_time]
            >= subscription_links[Colname.from_date]
        )
        & (
            subscription_master_data_and_prices[Colname.charge_time]
            < subscription_links[Colname.to_date]
        ),
        how="inner",
    ).select(
        subscription_master_data_and_prices[Colname.charge_key],
        subscription_master_data_and_prices[Colname.charge_type],
        subscription_master_data_and_prices[Colname.charge_owner],
        subscription_master_data_and_prices[Colname.charge_code],
        subscription_master_data_and_prices[Colname.charge_time],
        subscription_master_data_and_prices[Colname.charge_price],
        subscription_links[Colname.metering_point_type],
        subscription_links[Colname.settlement_method],
        subscription_links[Colname.grid_area],
        subscription_links[Colname.energy_supplier_id],
    )

    return subscriptions
