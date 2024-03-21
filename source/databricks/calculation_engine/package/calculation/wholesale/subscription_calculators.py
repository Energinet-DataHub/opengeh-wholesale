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

from package.calculation.preparation.data_structures.prepared_subscriptions import (
    PreparedSubscriptions,
)
from package.calculation.wholesale.data_structures.wholesale_results import (
    WholesaleResults,
)

from package.calculation.wholesale.calculate_total_quantity_and_amount import (
    calculate_total_quantity_and_amount,
)
from package.codelists import ChargeType
from package.constants import Colname


def calculate(
    prepared_subscriptions: PreparedSubscriptions,
    calculation_period_start: datetime,
    calculation_period_end: datetime,
    time_zone: str,
) -> WholesaleResults:
    prepared_subscriptions_df = prepared_subscriptions.df
    subscriptions_with_daily_price = _calculate_price_per_day(
        prepared_subscriptions_df,
        calculation_period_start,
        calculation_period_end,
        time_zone,
    )

    subscription_amount_per_charge = calculate_total_quantity_and_amount(
        subscriptions_with_daily_price, charge_type=ChargeType.SUBSCRIPTION
    )

    return WholesaleResults(subscription_amount_per_charge)


def _calculate_price_per_day(
    prepared_subscriptions: DataFrame,
    calculation_period_start: datetime,
    calculation_period_end: datetime,
    time_zone: str,
) -> DataFrame:
    time_zone_info = ZoneInfo(time_zone)
    period_start_local_time = calculation_period_start.astimezone(time_zone_info)
    period_end_local_time = calculation_period_end.astimezone(time_zone_info)
    days_in_month = (period_end_local_time - period_start_local_time).days

    subscriptions_with_daily_price = prepared_subscriptions.withColumn(
        Colname.charge_price, (f.col(Colname.charge_price) / f.lit(days_in_month))
    )

    return subscriptions_with_daily_price
