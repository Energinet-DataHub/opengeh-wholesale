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
from typing import cast

from pyspark.sql import DataFrame

from package.calculation.calculation_results import WholesaleResultsContainer
from package.calculation.wholesale.data_structures import MonthlyAmountPerCharge


def get_all_monthly_amounts_per_charge(
    results: WholesaleResultsContainer,
) -> MonthlyAmountPerCharge:

    monthly_tariff_from_daily = cast(
        DataFrame, results.monthly_tariff_from_daily_per_ga_co_es
    )
    monthly_tariff_from_hourly = cast(
        DataFrame, results.monthly_tariff_from_hourly_per_ga_co_es
    )
    monthly_subscription = cast(DataFrame, results.monthly_subscription_per_ga_co_es)
    monthly_fee = cast(DataFrame, results.monthly_fee_per_ga_co_es)

    monthly_amount_per_charge_df = (
        monthly_tariff_from_daily.union(monthly_tariff_from_hourly)
        .union(monthly_subscription)
        .union(monthly_fee)
    )

    return MonthlyAmountPerCharge(monthly_amount_per_charge_df)
