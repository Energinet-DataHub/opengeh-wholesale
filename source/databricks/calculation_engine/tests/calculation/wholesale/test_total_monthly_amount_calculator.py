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

from decimal import Decimal

from pyspark.sql import SparkSession
from package.codelists import (
    ChargeType,
)
from package.constants import Colname
import tests.calculation.wholesale.wholesale_results_factory as wholesale_results_factory
from package.calculation.wholesale.total_monthly_amount_calculator import calculate


SYSTEM_OPERATOR_ID = "system_operator_id"
GRID_PROVIDER_ID = "system_operator_id"


def test__calculate__when_no_charge_tax_in_group__sums_all_amounts(
    spark: SparkSession,
) -> None:
    # Arrange
    monthly_amounts_rows = [
        wholesale_results_factory.create_monthly_amount_row(
            charge_type=ChargeType.TARIFF,
            total_amount=Decimal("1"),
            charge_tax=False,
        ),
        wholesale_results_factory.create_monthly_amount_row(
            charge_type=ChargeType.FEE,
            total_amount=Decimal("1"),
            charge_tax=False,
        ),
        wholesale_results_factory.create_monthly_amount_row(
            charge_type=ChargeType.SUBSCRIPTION,
            total_amount=Decimal("1"),
            charge_tax=False,
        ),
    ]
    monthly_amounts = wholesale_results_factory.create(spark, monthly_amounts_rows)

    # Act
    actual = calculate(
        monthly_amounts.df,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("3.000000")
    assert actual.count() == 1


def test__calculate__tax_amount_is_added_to_other_charge_owners(
    spark: SparkSession,
) -> None:
    # Arrange
    monthly_tariffs_from_hourly = _create_default_monthly_tariff(spark)
    monthly_tariffs_from_daily = _create_default_monthly_tariff(spark)
    monthly_fees = _create_default_monthly_fee(spark)
    monthly_subscriptions = _create_default_monthly_subscription(spark)

    monthly_tariffs_from_hourly = (
        monthly_tariffs_from_hourlywholesale_results_factory.create(
            spark,
            [
                monthly_tariffs_from_hourly,
                wholesale_results_factory.create_monthly_amount_row(
                    charge_type=ChargeType.TARIFF,
                    total_amount=Decimal("5"),
                    charge_owner=SYSTEM_OPERATOR_ID,
                    charge_tax=True,
                ),
            ],
        )
    )
    monthly_subscriptions = wholesale_results_factory.create(
        spark,
        wholesale_results_factory.create_monthly_amount_row(
            charge_type=ChargeType.SUBSCRIPTION,
            total_amount=Decimal("4"),
            charge_tax=False,
        ),
    )

    # Act
    actual = calculate(
        monthly_subscriptions=monthly_subscriptions.df,
        monthly_fees=monthly_fees.df,
        monthly_tariffs_from_hourly=monthly_tariffs_from_hourly.df,
        monthly_tariffs_from_daily=monthly_tariffs_from_daily.df,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("10.000000")
    assert actual.count() == 1
