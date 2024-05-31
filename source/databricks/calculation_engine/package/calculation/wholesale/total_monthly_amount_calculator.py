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


def calculate_per_ga_co_es(
    monthly_amounts_per_charge: MonthlyAmountPerCharge,
) -> TotalMonthlyAmount:
    """
    Calculates the total monthly amount for each group of grid area, charge owner and charge time.
    Rows that are tax amounts are added to the other rows - but only the rows where the charge owner is not the tax owner itself.
    """

    total_amount_without_tax = _calculate_total_amount_for_charge_tax_value(
        monthly_amounts_per_charge, charge_tax=False
    )

    total_amount_with_tax = _calculate_total_amount_for_charge_tax_value(
        monthly_amounts_per_charge, charge_tax=True
    )

    amount_without_tax = "amount_without_tax"
    amount_with_tax = "amount_with_tax"
    tax_charge_owner = "tax_charge_owner"

    total_amount_with_tax = total_amount_with_tax.withColumnRenamed(
        Colname.charge_owner, tax_charge_owner
    )

    total_monthly_amount = total_amount_without_tax.join(
        total_amount_with_tax,
        [Colname.grid_area_code, Colname.energy_supplier_id],
        "left",
    ).select(
        total_amount_without_tax[Colname.grid_area_code],
        total_amount_without_tax[Colname.charge_owner],
        total_amount_without_tax[Colname.energy_supplier_id],
        total_amount_without_tax[Colname.charge_time],
        total_amount_without_tax[Colname.total_amount].alias(amount_without_tax),
        total_amount_with_tax[Colname.total_amount].alias(amount_with_tax),
        total_amount_with_tax[tax_charge_owner],
    )

    total_monthly_amount = total_monthly_amount.withColumn(
        Colname.total_amount,
        f.when(
            f.col(Colname.charge_owner) == f.col(tax_charge_owner),
            f.col(amount_without_tax),
        ).otherwise(
            f.when(
                (f.col(amount_with_tax).isNull())
                & (f.col(amount_without_tax).isNull()),
                None,
            ).otherwise(
                f.coalesce(f.col(amount_with_tax), f.lit(0))
                + f.coalesce(f.col(amount_without_tax), f.lit(0))
            )
        ),
    )

    return TotalMonthlyAmount(total_monthly_amount)


def calculate_per_ga_es(
    total_monthly_amount_per_ga_co_es: TotalMonthlyAmount,
) -> TotalMonthlyAmount:
    total_monthly_amount_per_ga_es = (
        total_monthly_amount_per_ga_co_es.df.groupBy(
            Colname.grid_area_code, Colname.energy_supplier_id, Colname.charge_time
        )
        .agg(f.sum(Colname.total_amount).alias(Colname.total_amount))
        .withColumn(Colname.charge_owner, f.lit(None).cast(StringType()))
    )
    return TotalMonthlyAmount(total_monthly_amount_per_ga_es)


def _calculate_total_amount_for_charge_tax_value(
    monthly_amount_per_charge: MonthlyAmountPerCharge, charge_tax: bool
) -> DataFrame:

    monthly_amounts_with_tax = monthly_amount_per_charge.df.where(
        f.col(Colname.charge_tax) == charge_tax  # noqa: E712
    )

    total_monthly_amount_for_charge_type = monthly_amounts_with_tax.groupBy(
        Colname.grid_area_code,
        Colname.charge_owner,
        Colname.energy_supplier_id,
    ).agg(
        f.sum(Colname.total_amount).alias(Colname.total_amount),
        f.first(Colname.charge_tax).alias(Colname.charge_tax),
        f.first(Colname.charge_time).alias(Colname.charge_time),
    )

    return total_monthly_amount_for_charge_type
