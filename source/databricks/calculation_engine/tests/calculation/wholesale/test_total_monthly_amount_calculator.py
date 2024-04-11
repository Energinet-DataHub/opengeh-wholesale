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
import calculation.wholesale.factories.monthly_amount_per_charge_factory as monthly_amount_per_charge_factory
from package.calculation.wholesale.total_monthly_amount_calculator import (
    calculate_per_charge_owner,
)


SYSTEM_OPERATOR_ID = "system_operator_id"
GRID_ACCESS_PROVIDER_ID = "grid_access_provider_id"


def test__calculate_per_charge_owner__sums_across_charge_types(
    spark: SparkSession,
) -> None:
    # Arrange
    monthly_amounts_rows = [
        monthly_amount_per_charge_factory.create_row(
            charge_type=ChargeType.TARIFF,
            total_amount=Decimal("1"),
            charge_tax=False,
        ),
        monthly_amount_per_charge_factory.create_row(
            charge_type=ChargeType.FEE,
            total_amount=Decimal("1"),
            charge_tax=False,
        ),
        monthly_amount_per_charge_factory.create_row(
            charge_type=ChargeType.SUBSCRIPTION,
            total_amount=Decimal("1"),
            charge_tax=False,
        ),
    ]
    monthly_amounts = monthly_amount_per_charge_factory.create(
        spark, monthly_amounts_rows
    )

    # Act
    actual = calculate_per_charge_owner(
        monthly_amounts,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("3.000000")
    assert actual.count() == 1


def test__calculate_per_charge_owner__sums_across_energy_suppliers(
    spark: SparkSession,
) -> None:
    # Arrange
    monthly_amounts_rows = [
        monthly_amount_per_charge_factory.create_row(
            charge_type=ChargeType.SUBSCRIPTION,
            total_amount=Decimal("1"),
            charge_tax=False,
            charge_owner=GRID_ACCESS_PROVIDER_ID,
            energy_supplier_id="1",
        ),
        monthly_amount_per_charge_factory.create_row(
            charge_type=ChargeType.SUBSCRIPTION,
            total_amount=Decimal("1"),
            charge_tax=False,
            charge_owner=GRID_ACCESS_PROVIDER_ID,
            energy_supplier_id="2",
        ),
    ]
    monthly_amounts = monthly_amount_per_charge_factory.create(
        spark, monthly_amounts_rows
    )

    # Act
    actual = calculate_per_charge_owner(
        monthly_amounts,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("2.000000")
    assert actual.count() == 1


@pytest.mark.parametrize(
    "charge_owner, expected",
    [
        [SYSTEM_OPERATOR_ID, Decimal("1.000000")],
        [GRID_ACCESS_PROVIDER_ID, Decimal("2.000000")],
    ],
)
def test__calculate_per_charge_owner__adds_tax_amount_only_to_grid_access_operator(
    spark: SparkSession, charge_owner: str, expected: Decimal
) -> None:
    # Arrange
    monthly_amounts_rows = [
        monthly_amount_per_charge_factory.create_row(
            charge_type=ChargeType.SUBSCRIPTION,
            total_amount=Decimal("1"),
            charge_tax=False,
            charge_owner=charge_owner,
        ),
        monthly_amount_per_charge_factory.create_row(
            charge_type=ChargeType.TARIFF,
            total_amount=Decimal("1"),
            charge_tax=True,
            charge_owner=SYSTEM_OPERATOR_ID,
        ),
    ]
    monthly_amounts = monthly_amount_per_charge_factory.create(
        spark, monthly_amounts_rows
    )

    # Act
    actual = calculate_per_charge_owner(
        monthly_amounts,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == expected
    assert actual.count() == 1


@pytest.mark.parametrize(
    "amount_without_tax, amount_with_tax, expected",
    [
        [None, Decimal("1.000000"), Decimal("1.000000")],
        [Decimal("1.000000"), None, Decimal("1.000000")],
        [None, None, None],
    ],
)
def test__calculate_per_charge_owner__ignores_null_in_sum(
    spark: SparkSession,
    amount_without_tax: Decimal,
    amount_with_tax: Decimal,
    expected: Decimal,
) -> None:
    # Arrange
    monthly_amounts_rows = [
        monthly_amount_per_charge_factory.create_row(
            charge_type=ChargeType.SUBSCRIPTION,
            total_amount=amount_without_tax,
            charge_tax=False,
            charge_owner=GRID_ACCESS_PROVIDER_ID,
        ),
        monthly_amount_per_charge_factory.create_row(
            charge_type=ChargeType.TARIFF,
            total_amount=amount_with_tax,
            charge_tax=True,
            charge_owner=SYSTEM_OPERATOR_ID,
        ),
    ]
    monthly_amounts = monthly_amount_per_charge_factory.create(
        spark, monthly_amounts_rows
    )

    # Act
    actual = calculate_per_charge_owner(
        monthly_amounts,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == expected
    assert actual.count() == 1
