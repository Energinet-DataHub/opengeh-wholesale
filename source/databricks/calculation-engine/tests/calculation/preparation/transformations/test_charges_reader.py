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
from pyspark.sql import SparkSession, DataFrame
from package.calculation.preparation.transformations.charges_reader import (
    _join_with_charge_prices,
    _join_with_charge_links,
)
from package.calculation.wholesale.schemas.charges_schema import (
    charges_schema,
    charge_prices_schema,
    charge_links_schema,
)
from tests.helpers.test_schemas import (
    charges_with_prices_schema,
)
import pytest


DEFAULT_FROM_DATE = datetime(2020, 1, 1, 0, 0)
DEFAULT_TO_DATE = datetime(2020, 2, 1, 0, 0)
DEFAULT_TIME = datetime(2020, 1, 1, 0, 0)

charges_dataset = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        "P1D",
        "No",
        "DDK",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
    )
]

charges_with_prices_dataset_1 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        "P1D",
        "No",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
        datetime(2020, 1, 15, 0, 0),
        Decimal("200.50"),
    )
]
charges_with_prices_dataset_2 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        "P1D",
        "No",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
        datetime(2021, 2, 1, 0, 0),
        Decimal("200.50"),
    )
]
charges_with_prices_dataset_3 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        "P1D",
        "No",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
        datetime(2020, 1, 1, 0, 0),
        Decimal("200.50"),
    )
]
charges_with_prices_dataset_4 = [
    (
        "001-D01-002",
        "001",
        "D01",
        "001",
        "P1D",
        "No",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
        datetime(2020, 1, 15, 0, 0),
        Decimal("200.50"),
    )
]

charge_prices_dataset = [
    ("001-D01-001", Decimal("200.50"), datetime(2020, 1, 2, 0, 0)),
    ("001-D01-001", Decimal("100.50"), datetime(2020, 1, 5, 0, 0)),
    ("001-D01-002", Decimal("100.50"), datetime(2020, 1, 6, 0, 0)),
]

charge_links_dataset = [
    ("001-D01-001", "D01", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0))
]


@pytest.mark.parametrize(
    "charges,charge_prices,expected", [(charges_dataset, charge_prices_dataset, 2)]
)
def test__join_with_charge_prices__joins_on_charge_key(
    spark: SparkSession, charges: DataFrame, charge_prices: DataFrame, expected: int
) -> None:
    # Arrange
    charges = spark.createDataFrame(charges, schema=charges_schema)
    charge_prices = spark.createDataFrame(charge_prices, schema=charge_prices_schema)

    # Act
    result = _join_with_charge_prices(charges, charge_prices)

    # Assert
    assert result.count() == expected


@pytest.mark.parametrize(
    "charges_with_prices,charge_links,expected",
    [
        (charges_with_prices_dataset_1, charge_links_dataset, 1),
        (charges_with_prices_dataset_2, charge_links_dataset, 0),
        (charges_with_prices_dataset_3, charge_links_dataset, 1),
        (charges_with_prices_dataset_4, charge_links_dataset, 0),
    ],
)
def test__join_with_charge_links__joins_on_charge_key_and_time_is_between_from_and_to_date(
    spark: SparkSession,
    charges_with_prices: DataFrame,
    charge_links: DataFrame,
    expected: int,
) -> None:
    # Arrange
    charges_with_prices = spark.createDataFrame(
        charges_with_prices, schema=charges_with_prices_schema
    )
    charge_links = spark.createDataFrame(charge_links, schema=charge_links_schema)

    # Act
    result = _join_with_charge_links(charges_with_prices, charge_links)

    # Assert
    assert result.count() == expected
