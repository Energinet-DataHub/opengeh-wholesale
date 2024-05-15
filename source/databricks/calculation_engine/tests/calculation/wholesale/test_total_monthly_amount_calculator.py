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
from decimal import Decimal

import pytest
from pyspark.sql import SparkSession
import pyspark.sql.functions as f
from package.codelists import (
    ChargeType,
)
from package.constants import Colname
import calculation.wholesale.factories.monthly_amount_per_charge_factory as monthly_amount_per_charge_factory
import calculation.wholesale.factories.total_monthly_amount_factory as total_monthly_amount_factory
from package.calculation.wholesale.total_monthly_amount_calculator import (
    calculate_per_ga_co_es,
    calculate_per_ga_es,
)


SYSTEM_OPERATOR_ID = "system_operator_id"
GRID_ACCESS_PROVIDER_ID = "grid_access_provider_id"


def test__calculate_per_ga_co_es__sums_across_charge_types(
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
    actual = calculate_per_ga_co_es(
        monthly_amounts,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("3.000000")
    assert actual.count() == 1


def test__calculate_per_ga_co_es__sums_per_energy_supplier(
    spark: SparkSession,
) -> None:
    # Arrange
    monthly_amounts_rows = [
        monthly_amount_per_charge_factory.create_row(
            total_amount=Decimal("1"),
            charge_tax=False,
            energy_supplier_id="1",
        ),
        monthly_amount_per_charge_factory.create_row(
            total_amount=Decimal("2"),
            charge_tax=False,
            energy_supplier_id="2",
        ),
        monthly_amount_per_charge_factory.create_row(
            total_amount=Decimal("3"),
            charge_tax=False,
            energy_supplier_id="2",
        ),
    ]
    monthly_amounts = monthly_amount_per_charge_factory.create(
        spark, monthly_amounts_rows
    )

    # Act
    actual = calculate_per_ga_co_es(
        monthly_amounts,
    ).df

    # Assert
    actual.show()
    assert actual.count() == 2
    assert actual.where(f.col(Colname.energy_supplier_id) == "1").collect()[0][
        Colname.total_amount
    ] == Decimal("1.000000")
    assert actual.where(f.col(Colname.energy_supplier_id) == "2").collect()[0][
        Colname.total_amount
    ] == Decimal("5.000000")


@pytest.mark.parametrize(
    "charge_owner, expected",
    [
        [SYSTEM_OPERATOR_ID, Decimal("1.000000")],
        [GRID_ACCESS_PROVIDER_ID, Decimal("2.000000")],
    ],
)
def test__calculate_per_ga_co_es__adds_tax_amount_only_to_grid_access_operator(
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
    actual = calculate_per_ga_co_es(
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
def test__calculate_per_ga_co_es__ignores_null_in_sum(
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
    actual = calculate_per_ga_co_es(
        monthly_amounts,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == expected
    assert actual.count() == 1


def test__calculate_per_ga_co_es__when_multiple_charge_owners_with_multiple_energy_suppliers__return_expected_number_of_rows(
    spark: SparkSession,
) -> None:
    # Arrange
    monthly_amounts_rows = [
        monthly_amount_per_charge_factory.create_row(
            charge_tax=False,
            charge_owner="1",
            energy_supplier_id="1",
        ),
        monthly_amount_per_charge_factory.create_row(
            charge_tax=False,
            charge_owner="1",
            energy_supplier_id="2",
        ),
        monthly_amount_per_charge_factory.create_row(
            charge_tax=False,
            charge_owner="2",
            energy_supplier_id="1",
        ),
        monthly_amount_per_charge_factory.create_row(
            charge_tax=False,
            charge_owner="2",
            energy_supplier_id="2",
        ),
        monthly_amount_per_charge_factory.create_row(
            charge_tax=False,
            charge_owner=SYSTEM_OPERATOR_ID,
            energy_supplier_id="1",
        ),
        monthly_amount_per_charge_factory.create_row(
            charge_tax=False,
            charge_owner=SYSTEM_OPERATOR_ID,
            energy_supplier_id="2",
        ),
        monthly_amount_per_charge_factory.create_row(
            charge_tax=True,
            charge_owner=SYSTEM_OPERATOR_ID,
            energy_supplier_id="1",
        ),
        monthly_amount_per_charge_factory.create_row(
            charge_tax=True,
            charge_owner=SYSTEM_OPERATOR_ID,
            energy_supplier_id="2",
        ),
    ]
    monthly_amounts = monthly_amount_per_charge_factory.create(
        spark, monthly_amounts_rows
    )

    # Act
    actual = calculate_per_ga_co_es(
        monthly_amounts,
    ).df

    # Assert
    assert actual.count() == 6


def test__calculate_per_ga_co_es__when_tax_charge_has_other_energy_supplier__ignores_tax_charge_in_sum(
    spark: SparkSession,
) -> None:
    # Arrange
    monthly_amounts_rows = [
        monthly_amount_per_charge_factory.create_row(
            charge_tax=False,
            charge_owner=GRID_ACCESS_PROVIDER_ID,
            energy_supplier_id="1",
            total_amount=Decimal("2"),
        ),
        monthly_amount_per_charge_factory.create_row(
            charge_tax=True,
            charge_owner=SYSTEM_OPERATOR_ID,
            energy_supplier_id="2",
            total_amount=Decimal("3"),
        ),
    ]
    monthly_amounts = monthly_amount_per_charge_factory.create(
        spark, monthly_amounts_rows
    )

    # Act
    actual = calculate_per_ga_co_es(
        monthly_amounts,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("2.000000")
    assert actual.count() == 1


@pytest.mark.parametrize(
    "grid_area, energy_supplier_id, total_amount, charge_owner, charge_time, expected_count, expected_total_amount",
    [
        [  # match on grid_area, energy_supplier_id, charge_time
            "123",
            "es_id_1",
            Decimal("1"),
            "co_id_1",
            datetime(2021, 1, 1),
            1,
            Decimal("3"),
        ],
        [  # match on grid_area, energy_supplier_id
            "123",
            "es_id_1",
            Decimal("1"),
            "co_id_1",
            datetime(2021, 2, 1),
            2,
            Decimal("1"),
        ],
        [  # # match on energy_supplier_id, charge_time
            "321",
            "es_id_1",
            Decimal("1"),
            "co_id_1",
            datetime(2021, 1, 1),
            2,
            Decimal("1"),
        ],
        [  # match on grid_area, charge_time
            "123",
            "es_id_2",
            Decimal("1"),
            "co_id_1",
            datetime(2021, 1, 1),
            2,
            Decimal("1"),
        ],
        [  # match on none
            "321",
            "es_id_2",
            Decimal("1"),
            "co_id_1",
            datetime(2021, 2, 1),
            2,
            Decimal("1"),
        ],
    ],
)
def test__calculate_per_ga_es__when_grid_area_and_energy_supplier_and_charge_time__group_them_and_ignore_charge_owner(
    grid_area: str,
    energy_supplier_id: str,
    total_amount: Decimal,
    charge_owner: str,
    charge_time: datetime,
    expected_count: int,
    expected_total_amount: Decimal,
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        total_monthly_amount_factory.create_row(
            grid_area=grid_area,
            energy_supplier_id=energy_supplier_id,
            total_amount=total_amount,
            charge_owner=charge_owner,
            charge_time=charge_time,
        ),
        total_monthly_amount_factory.create_row(
            grid_area="123",
            energy_supplier_id="es_id_1",
            total_amount=Decimal("2"),
            charge_owner="co_id_1",
            charge_time=datetime(2021, 1, 1),
        ),
    ]
    total_monthly_amount_per_ga_co_es = total_monthly_amount_factory.create(spark, rows)

    # Act
    actual = calculate_per_ga_es(
        total_monthly_amount_per_ga_co_es,
    )

    # Assert
    assert actual.df.count() == expected_count
    assert actual.df.collect()[0][Colname.total_amount] == expected_total_amount
    assert actual.df.collect()[0][Colname.charge_owner] is None
