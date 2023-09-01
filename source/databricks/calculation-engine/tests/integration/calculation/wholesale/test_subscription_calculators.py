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
from datetime import datetime
from tests.helpers.test_schemas import (
    charges_flex_consumption_schema,
    charges_per_day_schema,
)

from package.calculation.wholesale.subscription_calculators import (
    calculate_daily_subscription_price,
    calculate_price_per_day,
    filter_on_metering_point_type_and_settlement_method,
    get_count_of_charges_and_total_daily_charge_price,
)
from package.calculation.wholesale.wholesale_initializer import get_subscription_charges
from calendar import monthrange
import pytest
from package.constants import Colname


subscription_charges_dataset_1 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("200.50"),
        datetime(2020, 1, 1, 0, 0),
        "E17",
        "D01",
        1,
        1,
    )
]
subscription_charges_dataset_2 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("200.50"),
        datetime(2020, 2, 1, 0, 0),
        "E18",
        "001",
        1,
        1,
    )
]
subscription_charges_dataset_3 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("200.50"),
        datetime(2020, 2, 1, 0, 0),
        "E17",
        "001",
        1,
        1,
    )
]
subscription_charges_dataset_4 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("200.50"),
        datetime(2020, 2, 1, 0, 0),
        "E18",
        "001",
        1,
        1,
    )
]


@pytest.mark.parametrize(
    "subscription_charges,expected",
    [
        (subscription_charges_dataset_1, 1),
        (subscription_charges_dataset_2, 0),
        (subscription_charges_dataset_3, 0),
        (subscription_charges_dataset_4, 0),
    ],
)
def test__filter_on_metering_point_type_and_settlement_method__filters_on_E17_and_D01(
    spark, subscription_charges, expected
):
    # Arrange
    subscription_charges = spark.createDataFrame(
        subscription_charges, schema=charges_flex_consumption_schema
    )  # subscription_charges and charges_flex_consumption has the same schema
    # Act
    result = filter_on_metering_point_type_and_settlement_method(subscription_charges)

    # Assert
    assert result.count() == expected


charges_flex_consumption_dataset_1 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2020, 1, 1, 0, 0),
        "E17",
        "001",
        1,
        1,
    )
]
charges_flex_consumption_dataset_2 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("200.50"),
        datetime(2020, 2, 1, 0, 0),
        "E17",
        "001",
        1,
        1,
    )
]


@pytest.mark.parametrize(
    "charges_flex_consumption,expected",
    [
        (charges_flex_consumption_dataset_1, Decimal("3.22903226")),
        (charges_flex_consumption_dataset_2, Decimal("6.91379310")),
    ],
)
def test__calculate_price_per_day__divides_charge_price_by_days_in_month(
    spark, charges_flex_consumption, expected
):
    # Arrange
    charges_flex_consumption = spark.createDataFrame(
        charges_flex_consumption, schema=charges_flex_consumption_schema
    )

    # Act
    result = calculate_price_per_day(charges_flex_consumption)

    # Assert
    assert result.collect()[0][Colname.price_per_day] == expected


charges_per_day_dataset_1 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2020, 1, 1, 0, 0),
        "E17",
        "001",
        1,
        1,
        Decimal("3.22903226"),
    )
]
charges_per_day_dataset_2 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2020, 1, 1, 0, 0),
        "E17",
        "001",
        1,
        1,
        Decimal("3.22903226"),
    ),
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2020, 1, 1, 0, 0),
        "E17",
        "001",
        1,
        1,
        Decimal("3.22903226"),
    ),
]
charges_per_day_dataset_3 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2020, 1, 1, 0, 0),
        "E17",
        "001",
        1,
        1,
        Decimal("3.22903226"),
    ),
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2020, 1, 2, 0, 0),
        "E17",
        "001",
        1,
        1,
        Decimal("3.22903226"),
    ),
]
charges_per_day_dataset_4 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2020, 1, 1, 0, 0),
        "E17",
        "001",
        1,
        1,
        Decimal("3.22903226"),
    ),
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2021, 1, 1, 0, 0),
        "E17",
        "001",
        1,
        1,
        Decimal("3.22903226"),
    ),
]


@pytest.mark.parametrize(
    "charges_per_day,expected_charge_count,expected_total_daily_charge_price",
    [
        (charges_per_day_dataset_1, 1, Decimal("3.22903226")),
        (charges_per_day_dataset_2, 2, Decimal("6.45806452")),
        (charges_per_day_dataset_3, 1, Decimal("3.22903226")),
        (charges_per_day_dataset_4, 1, Decimal("3.22903226")),
    ],
)
def test__get_count_of_charges_and_total_daily_charge_price__counts_and_sums_up_amount_per_day(
    spark, charges_per_day, expected_charge_count, expected_total_daily_charge_price
):
    # Arrange
    charges_per_day = spark.createDataFrame(
        charges_per_day, schema=charges_per_day_schema
    )

    # Act
    result = get_count_of_charges_and_total_daily_charge_price(charges_per_day)

    # Assert
    result_collect = result.collect()
    assert result_collect[0][Colname.charge_count] == expected_charge_count
    assert (
        result_collect[0][Colname.total_daily_charge_price]
        == expected_total_daily_charge_price
    )
