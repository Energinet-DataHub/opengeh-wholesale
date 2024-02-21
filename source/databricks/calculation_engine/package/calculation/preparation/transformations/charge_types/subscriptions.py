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
from package.calculation.preparation.charge_period_prices import ChargePeriodPrices
from package.codelists import ChargeType
from package.constants import Colname


def get_subscription_charges(
    charge_period_prices: ChargePeriodPrices,
    charge_link_metering_point_periods: ChargeLinkMeteringPointPeriods,
    time_zone: str,
) -> DataFrame:
    charge_link_metering_points_df = charge_link_metering_point_periods.df

    subscription_charges = charge_period_prices.df.filter(
        f.col(Colname.charge_type) == ChargeType.SUBSCRIPTION.value
    )

    subscription_charges = _explode_subscription(subscription_charges, time_zone)

    subscriptions = subscription_charges.join(
        charge_link_metering_points_df,
        (
            subscription_charges[Colname.charge_key]
            == charge_link_metering_points_df[Colname.charge_key]
        )
        & (
            subscription_charges[Colname.charge_time]
            >= charge_link_metering_points_df[Colname.from_date]
        )
        & (
            subscription_charges[Colname.charge_time]
            < charge_link_metering_points_df[Colname.to_date]
        ),
        how="inner",
    ).select(
        subscription_charges[Colname.charge_key],
        Colname.charge_code,
        Colname.charge_type,
        Colname.charge_owner,
        Colname.charge_time,
        Colname.charge_price,
        charge_link_metering_points_df[Colname.metering_point_type],
        charge_link_metering_points_df[Colname.settlement_method],
        charge_link_metering_points_df[Colname.grid_area],
        charge_link_metering_points_df[Colname.energy_supplier_id],
    )
    subscriptions.show()

    return subscriptions


def _explode_subscription(
    charge_prices: DataFrame,
    time_zone: str,
) -> DataFrame:
    all_dates_df = (
        charge_prices.select(Colname.charge_key, Colname.from_date, Colname.to_date)
        .dropDuplicates()
        .select(
            Colname.charge_key,
            f.explode(
                f.expr(
                    f"sequence(from_utc_timestamp({Colname.from_date}, '{time_zone}'), from_utc_timestamp({Colname.to_date}, '{time_zone}'), interval 1 day)"
                )
            ).alias("charge_time_local"),
        )
        .withColumn(
            Colname.charge_time,
            f.to_utc_timestamp(f.col("charge_time_local"), time_zone),
        )
        .drop("charge_time_local")
    )
    w = Window.partitionBy(Colname.charge_key).orderBy(Colname.charge_time)

    result = all_dates_df.join(
        charge_prices, [Colname.charge_key, Colname.charge_time], "left"
    ).select(
        Colname.charge_key,
        Colname.charge_time,
        *[
            f.last(f.col(c), ignorenulls=True).over(w).alias(c)
            for c in charge_prices.columns
            if c not in (Colname.charge_key, Colname.charge_time)
        ],
    )

    return result
