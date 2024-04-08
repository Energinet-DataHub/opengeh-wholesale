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
from pyspark.sql import DataFrame

from package.calculation.wholesale.data_structures.total_montly_amount import (
    TotalMonthlyAmount,
)
from package.constants import Colname


def calculate(
    monthly_subscriptions: DataFrame,
    monthly_fees: DataFrame,
    monthly_tariffs_from_hourly: DataFrame,
    monthly_tariffs_from_daily: DataFrame,
) -> TotalMonthlyAmount:

    total_amount_with_tax = _calculate_total_amount_with_charge_tax(
        monthly_tariffs_from_daily, monthly_tariffs_from_hourly
    )

    total_amount_without_tax = _calculate_total_amount_without_charge_tax(
        monthly_fees,
        monthly_subscriptions,
        monthly_tariffs_from_daily,
        monthly_tariffs_from_hourly,
    )

    total_monthly_amount = total_amount_without_tax.join(
        total_amount_with_tax,
        [
            Colname.grid_area,
            Colname.charge_owner,
        ],
    ).select(
        total_amount_without_tax[Colname.grid_area],
        total_amount_without_tax[Colname.charge_owner],
        total_amount_without_tax[Colname.energy_supplier_id],
        total_amount_without_tax[Colname.charge_time],
        total_amount_without_tax[Colname.total_amount],
        # Add total amount with tax to total amount without tax if it exists
        total_amount_without_tax[Colname.total_amount]
        + f.when(
            total_amount_with_tax[Colname.total_amount].isNotNull(),
            total_amount_with_tax[Colname.total_amount],
        ).otherwise(0),
    )

    return TotalMonthlyAmount(total_monthly_amount)


def _calculate_total_amount_without_charge_tax(
    monthly_fees,
    monthly_subscriptions,
    monthly_tariffs_from_daily,
    monthly_tariffs_from_hourly,
):
    monthly_amounts_without_tax = (
        monthly_fees.union(monthly_subscriptions)
        .union(monthly_tariffs_from_daily.where(f.col(Colname.charge_tax) == False))
        .union(monthly_tariffs_from_hourly.where(f.col(Colname.charge_tax) == False))
    )
    total_amount_without_tax = monthly_amounts_without_tax.groupBy(
        Colname.grid_area,
        Colname.energy_supplier_id,
        Colname.charge_owner,
        Colname.charge_time,
    ).agg(
        f.sum(Colname.total_amount).alias(Colname.total_amount),
        f.first(Colname.charge_tax).alias(Colname.charge_tax),
    )
    return total_amount_without_tax


def _calculate_total_amount_with_charge_tax(
    monthly_tariffs_from_daily: DataFrame, monthly_tariffs_from_hourly: DataFrame
) -> DataFrame:

    # Only tariffs have tax
    monthly_amounts_with_tax = monthly_tariffs_from_daily.where(
        f.col(Colname.charge_tax) == True
    ).union(monthly_tariffs_from_hourly.where(f.col(Colname.charge_tax) == True))

    total_amount_with_tax = monthly_amounts_with_tax.groupBy(
        Colname.grid_area,
        Colname.energy_supplier_id,
        Colname.charge_owner,
        Colname.charge_time,
    ).agg(
        f.sum(Colname.total_amount).alias(Colname.total_amount),
        f.first(Colname.charge_tax).alias(Colname.charge_tax),
    )

    return total_amount_with_tax
