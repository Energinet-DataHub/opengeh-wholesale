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

import package.calculation.preparation.data_structures as d
from package.codelists import ChargeType, WholesaleResultResolution
from package.constants import Colname


def get_prepared_subscriptions(
    charge_master_data: d.ChargeMasterData,
    charge_prices: d.ChargePrices,
    charge_link_metering_point_periods: d.ChargeLinkMeteringPointPeriods,
    time_zone: str,
) -> d.PreparedSubscriptions:
    subscriptions_df = _prepare(
        charge_master_data,
        charge_prices,
        charge_link_metering_point_periods,
        time_zone,
        ChargeType.SUBSCRIPTION,
    )

    return d.PreparedSubscriptions(subscriptions_df)


def get_prepared_fees(
    charge_master_data: d.ChargeMasterData,
    charge_prices: d.ChargePrices,
    charge_link_metering_point_periods: d.ChargeLinkMeteringPointPeriods,
    time_zone: str,
) -> d.PreparedFees:
    fees_df = _prepare(
        charge_master_data,
        charge_prices,
        charge_link_metering_point_periods,
        time_zone,
        ChargeType.FEE,
    )

    return d.PreparedFees(fees_df)


def _prepare(
    charge_master_data: d.ChargeMasterData,
    charge_prices: d.ChargePrices,
    charge_link_metering_point_periods: d.ChargeLinkMeteringPointPeriods,
    time_zone: str,
    charge_type: ChargeType,
) -> DataFrame:
    """
    This method does the following:
    - Joins charge_master_data, charge_prices and charge_link_metering_point_periods
    - Filters the result to only include the defined charge type
    - Explodes the result from monthly to daily resolution (only reelevant for subscription charges, because fees have
    one day between to and from date on charge links)
    - Add missing charge prices (None) to the result
    """
    charge_links = charge_link_metering_point_periods.filter_by_charge_type(charge_type)
    charge_prices = charge_prices.filter_by_charge_type(charge_type)
    charge_master_data = charge_master_data.filter_by_charge_type(charge_type)

    charge_master_data_and_prices = _join_with_prices(
        charge_master_data, charge_prices, time_zone
    )
    charge_with_links = _join_with_links(charge_master_data_and_prices, charge_links.df)
    charge_with_links = charge_with_links.withColumn(
        Colname.resolution, f.lit(WholesaleResultResolution.DAY.value)
    )
    return charge_with_links


def _join_with_prices(
    charge_master_data: d.ChargeMasterData,
    charge_prices: d.ChargePrices,
    time_zone: str,
) -> DataFrame:
    """
    Join charge_master_data with charge_prices.
    This method also ensure
    - Missing charge prices will be set to None.
    - The charge price is the last known charge price for the charge key.
    """
    charge_prices = charge_prices.df
    charge_master_data = charge_master_data.df

    charge_master_data_with_charge_time = _explode_with_daily_charge_time(
        charge_master_data, time_zone
    )

    w = Window.partitionBy(Colname.charge_key, Colname.from_date).orderBy(
        Colname.charge_time
    )

    master_data_with_prices = (
        charge_master_data_with_charge_time.join(
            charge_prices, [Colname.charge_key, Colname.charge_time], "left"
        )
        .withColumn(
            Colname.charge_price,
            f.last(Colname.charge_price, ignorenulls=True).over(w),
        )
        .select(
            charge_master_data_with_charge_time[Colname.charge_key],
            charge_master_data_with_charge_time[Colname.charge_type],
            charge_master_data_with_charge_time[Colname.charge_owner],
            charge_master_data_with_charge_time[Colname.charge_code],
            charge_master_data_with_charge_time[Colname.from_date],
            charge_master_data_with_charge_time[Colname.to_date],
            charge_master_data_with_charge_time[Colname.resolution],
            charge_master_data_with_charge_time[Colname.charge_tax],
            charge_master_data_with_charge_time[Colname.charge_time],
            Colname.charge_price,
        )
    )
    master_data_with_prices.show()
    return master_data_with_prices


def _explode_with_daily_charge_time(
    charge_master_data: DataFrame, time_zone: str
) -> DataFrame:
    """
    Add charge_time column to charge_periods DataFrame.
    The charge_time column is created by exploding charge_periods using from_date and to_date with a resolution of 1 day.
    """

    charge_periods_with_charge_time = charge_master_data.withColumn(
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
    charge_master_data_and_prices: DataFrame,
    charge_links: DataFrame,
) -> DataFrame:
    subscriptions = charge_master_data_and_prices.join(
        charge_links,
        (
            charge_master_data_and_prices[Colname.charge_key]
            == charge_links[Colname.charge_key]
        )
        & (
            charge_master_data_and_prices[Colname.charge_time]
            >= charge_links[Colname.from_date]
        )
        & (
            charge_master_data_and_prices[Colname.charge_time]
            < charge_links[Colname.to_date]
        ),
        how="inner",
    ).select(
        charge_master_data_and_prices[Colname.charge_key],
        charge_master_data_and_prices[Colname.charge_type],
        charge_master_data_and_prices[Colname.charge_owner],
        charge_master_data_and_prices[Colname.charge_code],
        charge_master_data_and_prices[Colname.charge_time],
        charge_master_data_and_prices[Colname.charge_price],
        charge_master_data_and_prices[Colname.charge_tax],
        charge_links[Colname.charge_quantity],
        charge_links[Colname.metering_point_type],
        charge_links[Colname.metering_point_id],
        charge_links[Colname.settlement_method],
        charge_links[Colname.grid_area],
        charge_links[Colname.energy_supplier_id],
    )

    return subscriptions
