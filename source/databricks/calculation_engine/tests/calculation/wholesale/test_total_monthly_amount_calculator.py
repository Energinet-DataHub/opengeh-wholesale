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

import pytest
from pyspark.sql import SparkSession
from package.codelists import (
    ChargeType,
)
from package.constants import Colname
import tests.calculation.wholesale.wholesale_results_factory as wholesale_results_factory
from package.calculation.wholesale.total_monthly_amount_calculator import calculate


SYSTEM_OPERATOR_ID = "system_operator_id"
GRID_ACCESS_PROVIDER_ID = "grid_access_provider_id"


def test__calculate__when_all_monthly_amounts_are_without_tax__sums_all_amounts(
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


@pytest.mark.parametrize(
    "charge_owner, expected",
    [
        [SYSTEM_OPERATOR_ID, Decimal("1.000000")],
        [GRID_ACCESS_PROVIDER_ID, Decimal("2.000000")],
    ],
)
def test__calculate__adds_tax_amount_if_not_system_operator(
    spark: SparkSession, charge_owner: str, expected: Decimal
) -> None:
    # Arrange
    monthly_amounts_rows = [
        wholesale_results_factory.create_monthly_amount_row(
            charge_type=ChargeType.SUBSCRIPTION,
            total_amount=Decimal("1"),
            charge_tax=False,
            charge_owner=charge_owner,
        ),
        wholesale_results_factory.create_monthly_amount_row(
            charge_type=ChargeType.TARIFF,
            total_amount=Decimal("1"),
            charge_tax=True,
            charge_owner=SYSTEM_OPERATOR_ID,
        ),
    ]
    monthly_amounts = wholesale_results_factory.create(spark, monthly_amounts_rows)

    # Act
    actual = calculate(
        monthly_amounts.df,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == expected
    assert actual.count() == 1


@pytest.mark.parametrize(
    "non_tax_amount, tax_amount, expected",
    [
        [None, Decimal("1.000000"), Decimal("1.000000")],
        [Decimal("1.000000"), None, Decimal("1.000000")],
        [None, None, None],
    ],
)
def test__calculate__when_amount_is_null__ignore_null_in_sum(
    spark: SparkSession, non_tax_amount: Decimal, tax_amount: Decimal, expected: Decimal
) -> None:
    # Arrange
    monthly_amounts_rows = [
        wholesale_results_factory.create_monthly_amount_row(
            charge_type=ChargeType.SUBSCRIPTION,
            total_amount=non_tax_amount,
            charge_tax=False,
            charge_owner=GRID_ACCESS_PROVIDER_ID,
        ),
        wholesale_results_factory.create_monthly_amount_row(
            charge_type=ChargeType.TARIFF,
            total_amount=tax_amount,
            charge_tax=True,
            charge_owner=SYSTEM_OPERATOR_ID,
        ),
    ]
    monthly_amounts = wholesale_results_factory.create(spark, monthly_amounts_rows)

    # Act
    actual = calculate(
        monthly_amounts.df,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == expected
    assert actual.count() == 1
