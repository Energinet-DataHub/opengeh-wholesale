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
from pyspark.sql.types import StringType

from package.calculation.wholesale.data_structures import (
    MonthlyAmountPerCharge,
    TotalMonthlyAmount,
)
from package.constants import Colname


def calculate_per_charge_owner(
    monthly_amounts_per_charge: MonthlyAmountPerCharge,
) -> TotalMonthlyAmount:
    """
    Calculates the total monthly amount for each group of grid area, charge owner and charge time.
    Rows that are tax amounts are added to the other rows - but only the rows where the charge owner is not the tax owner itself.
    """

    total_amount_without_tax = _calculate_total_amount_for_tax_type(
        monthly_amounts_per_charge, charge_tax=False
    )

    total_amount_with_tax = _calculate_total_amount_for_tax_type(
        monthly_amounts_per_charge, charge_tax=True
    )

    amount_without_tax = "amount_without_tax"
    amount_with_tax = "amount_with_tax"

    total_monthly_amount = total_amount_without_tax.join(
        total_amount_with_tax,
        (
            total_amount_without_tax[Colname.grid_area]
            == total_amount_with_tax[Colname.grid_area]
        )
        & (
            total_amount_without_tax[Colname.charge_owner]
            != total_amount_with_tax[Colname.charge_owner]
        ),
        "left",
    ).select(
        total_amount_without_tax[Colname.grid_area],
        total_amount_without_tax[Colname.charge_owner],
        total_amount_without_tax[Colname.charge_time],
        total_amount_without_tax[Colname.total_amount].alias(amount_without_tax),
        total_amount_with_tax[Colname.total_amount].alias(amount_with_tax),
        f.lit(None).cast(StringType()).alias(Colname.energy_supplier_id),
    )

    # Add tax amount to non-tax amount (if it is not null)
    total_monthly_amount = total_monthly_amount.withColumn(
        Colname.total_amount,
        f.when(
            (f.col(amount_without_tax).isNotNull())
            & (f.col(amount_with_tax).isNotNull()),
            f.col(amount_with_tax) + f.col(amount_without_tax),
        )
        .when(
            (f.col(amount_with_tax).isNotNull()) & (f.col(amount_without_tax).isNull()),
            f.col(amount_with_tax),
        )
        .otherwise(f.col(amount_without_tax)),
    )

    return TotalMonthlyAmount(total_monthly_amount)


def _calculate_total_amount_for_tax_type(
    monthly_amount_per_charge: MonthlyAmountPerCharge, charge_tax: bool
) -> DataFrame:

    monthly_amounts_with_tax = monthly_amount_per_charge.df.where(
        f.col(Colname.charge_tax) == charge_tax  # noqa: E712
    )

    total_monthly_amount_for_charge_type = monthly_amounts_with_tax.groupBy(
        Colname.grid_area,
        Colname.charge_owner,
    ).agg(
        f.sum(Colname.total_amount).alias(Colname.total_amount),
        f.first(Colname.charge_tax).alias(Colname.charge_tax),
        f.first(Colname.charge_time).alias(Colname.charge_time),
    )

    return total_monthly_amount_for_charge_type
