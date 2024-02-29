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
from pyspark.sql.functions import col, lit, sum
from pyspark.sql.types import DecimalType
from package.constants import Colname


def calculate_daily_subscription_amount(
    subscription_charges: DataFrame,
    calculation_period_start: datetime,
    calculation_period_end: datetime,
    time_zone: str,
) -> DataFrame:
    # calculate price per day
    charges_per_day = calculate_price_per_day(
        subscription_charges,
        calculation_period_start,
        calculation_period_end,
        time_zone,
    )

    # get count of charges and total daily charge price
    subscription_result = _add_count_of_charges_and_total_daily_charge_price(
        charges_per_day
    )

    return subscription_result


def calculate_price_per_day(
    charges_per_day_flex_consumption: DataFrame,
    calculation_period_start: datetime,
    calculation_period_end: datetime,
    time_zone: str,
) -> DataFrame:
    days_in_month = _get_days_in_month(
        calculation_period_start, calculation_period_end, time_zone
    )

    print(days_in_month)
    charges_per_day = charges_per_day_flex_consumption.withColumn(
        Colname.charge_price,
        (col(Colname.charge_price) / lit(days_in_month)).cast(DecimalType(14, 6)),
    )
    return charges_per_day


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


def _add_count_of_charges_and_total_daily_charge_price(
    charges_per_day: DataFrame,
) -> DataFrame:
    df = charges_per_day.groupBy(
        Colname.charge_key,
        Colname.charge_type,
        Colname.charge_owner,
        Colname.charge_price,
        Colname.grid_area,
        Colname.energy_supplier_id,
        Colname.charge_time,
    ).agg(
        sum(Colname.charge_quantity).alias(Colname.charge_count),
        sum(Colname.charge_price).alias(Colname.total_amount),
    )

    return df
