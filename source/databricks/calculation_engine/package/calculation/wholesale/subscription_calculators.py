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
from zoneinfo import ZoneInfo

from pyspark.sql import DataFrame
import pyspark.sql.functions as f
from pyspark.sql.types import DecimalType, ArrayType, StringType

from package.calculation.preparation.data_structures.prepared_subscriptions import (
    PreparedSubscriptions,
)
from package.calculation.wholesale.calculate_total_quantity_and_amount import (
    calculate_total_quantity_and_amount,
)
from package.codelists import ChargeUnit
from package.constants import Colname


def calculate(
    prepared_subscriptions: PreparedSubscriptions,
    calculation_period_start: datetime,
    calculation_period_end: datetime,
    time_zone: str,
) -> DataFrame:
    prepared_subscriptions_df = prepared_subscriptions.df
    subscriptions_with_daily_price = _calculate_price_per_day(
        prepared_subscriptions_df,
        calculation_period_start,
        calculation_period_end,
        time_zone,
    )

    subscription_amount_per_charge = calculate_total_quantity_and_amount(
        subscriptions_with_daily_price
    )

    return subscription_amount_per_charge.select(
        Colname.energy_supplier_id,
        Colname.grid_area,
        Colname.charge_time,
        Colname.metering_point_type,
        Colname.settlement_method,
        Colname.charge_key,
        Colname.charge_code,
        Colname.charge_type,
        Colname.charge_owner,
        Colname.charge_tax,
        Colname.resolution,
        f.col(Colname.total_quantity).cast(DecimalType(18, 3)),
        f.round(Colname.charge_price, 6).alias(Colname.charge_price),
        f.round(Colname.total_amount, 6).alias(Colname.total_amount),
        f.lit(ChargeUnit.PIECES.value).alias(Colname.unit),
        Colname.qualities,
    )


def _calculate_price_per_day(
    prepared_subscriptions: DataFrame,
    calculation_period_start: datetime,
    calculation_period_end: datetime,
    time_zone: str,
) -> DataFrame:
    days_in_month = _get_days_in_month(
        calculation_period_start, calculation_period_end, time_zone
    )

    subscriptions_with_daily_price = prepared_subscriptions.withColumn(
        Colname.charge_price, (f.col(Colname.charge_price) / f.lit(days_in_month))
    )

    return subscriptions_with_daily_price


def _get_days_in_month(
    calculation_period_start: datetime, calculation_period_end: datetime, time_zone: str
) -> int:
    time_zone_info = ZoneInfo(time_zone)
    period_start_local_time = calculation_period_start.astimezone(time_zone_info)
    period_end_local_time = calculation_period_end.astimezone(time_zone_info)

    if not _is_full_month_and_at_midnight(
        period_start_local_time, period_end_local_time
    ):
        raise Exception(
            f"The calculation period must be a full month starting and ending at midnight local time ({time_zone})) ."
        )

    # return days in month of the start time
    return (period_end_local_time - period_start_local_time).days


def _is_full_month_and_at_midnight(
    period_start_local_time: datetime, period_end_local_time: datetime
) -> bool:
    return (
        period_start_local_time.time()
        == period_end_local_time.time()
        == datetime.min.time()
        and period_start_local_time.day == 1
        and period_end_local_time.day == 1
        and period_end_local_time.month == (period_start_local_time.month % 12) + 1
    )
