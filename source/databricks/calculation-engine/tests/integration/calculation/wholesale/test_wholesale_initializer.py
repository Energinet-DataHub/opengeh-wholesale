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

from datetime import datetime, timedelta
from decimal import Decimal
from pyspark.sql import SparkSession, DataFrame
from typing import Callable
from package.calculation.wholesale.wholesale_initializer import (
    join_with_charge_prices,
    join_with_charge_links,
    join_with_metering_points,
    explode_subscription,
    get_charges_based_on_resolution,
    get_tariff_charges,
    group_by_time_series_on_metering_point_id_and_resolution_and_sum_quantity,
    join_with_grouped_time_series,
    get_charges_based_on_charge_type,
)
from package.codelists import ChargeType, ChargeResolution
from package.calculation.wholesale.schemas.charges_schema import (
    charges_schema,
    charge_prices_schema,
    charge_links_schema,
)
from package.calculation_input.schemas import time_series_point_schema, metering_point_period_schema
from tests.helpers.test_schemas import (
    charges_with_prices_schema,
    charges_with_price_and_links_schema,
    charges_complete_schema,
)
from pyspark.sql.functions import col
import pytest
from package.constants import Colname

DEFAULT_FROM_DATE = datetime(2020, 1, 1, 0, 0)
DEFAULT_TO_DATE = datetime(2020, 2, 1, 0, 0)

charges_dataset = [
    (
        "001-D01-001",
        "001",
        ChargeType.TARIFF,
        "001",
        ChargeResolution.DAY.value,
        "No",
        "DDK",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
    ),
    (
        "001-D01-001",
        "001",
        ChargeType.TARIFF,
        "001",
        ChargeResolution.DAY.value,
        "No",
        "DDK",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
    ),
    (
        "001-D01-001",
        "001",
        ChargeType.TARIFF,
        "001",
        ChargeResolution.HOUR.value,
        "No",
        "DDK",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
    ),
    (
        "001-D01-001",
        "001",
        ChargeType.TARIFF,
        "001",
        ChargeResolution.MONTH.value,
        "No",
        "DDK",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
    ),
]


# Tariff only
@pytest.mark.parametrize(
    "charges,resolution_duration,expected",
    [
        (charges_dataset, ChargeResolution.HOUR, 1),
        (charges_dataset, ChargeResolution.DAY, 2),
    ],
)
def test__get_charges_based_on_resolution__filters_on_resolution_hour_or_day_only_for_tariff(
    spark: SparkSession, charges: DataFrame, resolution_duration: ChargeResolution, expected: int
) -> None:
    # Arrange
    charges = spark.createDataFrame(charges, schema=charges_schema)

    # Act
    result = get_charges_based_on_resolution(charges, resolution_duration)

    # Assert
    assert result.count() == expected


charges_dataset = [
    (
        "001-D01-001",
        "001",
        ChargeType.TARIFF,
        "001",
        ChargeResolution.DAY.value,
        "No",
        "DDK",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
    ),
    (
        "001-D01-001",
        "001",
        ChargeType.TARIFF,
        "001",
        ChargeResolution.DAY.value,
        "No",
        "DDK",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
    ),
    (
        "001-D01-001",
        "001",
        ChargeType.SUBSCRIPTION,
        "001",
        ChargeResolution.DAY.value,
        "No",
        "DDK",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
    ),
    (
        "001-D01-001",
        "001",
        ChargeType.SUBSCRIPTION,
        "001",
        ChargeResolution.DAY.value,
        "No",
        "DDK",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
    ),
    (
        "001-D01-001",
        "001",
        ChargeType.FEE,
        "001",
        ChargeResolution.DAY.value,
        "No",
        "DDK",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
    ),
    (
        "001-D01-001",
        "001",
        ChargeType.TARIFF,
        "001",
        ChargeResolution.DAY.value,
        "No",
        "DDK",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
    ),
]


# Shared
@pytest.mark.parametrize(
    "charges,charge_type,expected",
    [
        (charges_dataset, ChargeType.TARIFF, 3),
        (charges_dataset, ChargeType.SUBSCRIPTION, 2),
        (charges_dataset, ChargeType.FEE, 1),
    ],
)
def test__get_charges_based_on_charge_type__filters_on_one_charge_type(
    spark: SparkSession, charges: DataFrame, charge_type: str, expected: int
) -> None:
    # Arrange
    charges = spark.createDataFrame(charges, schema=charges_schema)

    # Act
    result = get_charges_based_on_charge_type(charges, charge_type)

    # Assert
    assert result.count() == expected


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
charge_prices_dataset = [
    ("001-D01-001", Decimal("200.50"), datetime(2020, 1, 2, 0, 0)),
    ("001-D01-001", Decimal("100.50"), datetime(2020, 1, 5, 0, 0)),
    ("001-D01-002", Decimal("100.50"), datetime(2020, 1, 6, 0, 0)),
]


# Shared
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
    result = join_with_charge_prices(charges, charge_prices)

    # Assert
    assert result.count() == expected


subscription_charges_with_prices_dataset_1 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        "P1D",
        "No",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
        datetime(2020, 1, 2, 0, 0),
        Decimal("200.50"),
    )
]
subscription_charges_with_prices_dataset_2 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        "P1D",
        "No",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
        datetime(2021, 1, 2, 0, 0),
        Decimal("200.50"),
    )
]
subscription_charges_with_prices_dataset_3 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        "P1D",
        "No",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 2, 0, 0),
        datetime(2020, 2, 15, 0, 0),
        Decimal("200.50"),
    )
]
subscription_charges_with_prices_dataset_4 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        "P1D",
        "No",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
        datetime(2020, 3, 1, 0, 0),
        Decimal("200.50"),
    )
]


# Subscription only
@pytest.mark.parametrize(
    "subscription_charges_with_prices,expected",
    [
        (subscription_charges_with_prices_dataset_1, 31),
        (subscription_charges_with_prices_dataset_2, 0),
        (subscription_charges_with_prices_dataset_3, 2),
        (subscription_charges_with_prices_dataset_4, 0),
    ],
)
def test__explode_subscription__explodes_into_rows_based_on_number_of_days_between_from_and_to_date(
    spark: SparkSession, subscription_charges_with_prices: DataFrame, expected: int
) -> None:
    # Arrange
    subscription_charges_with_prices = spark.createDataFrame(
        subscription_charges_with_prices, schema=charges_with_prices_schema
    )

    # Act
    result = explode_subscription(subscription_charges_with_prices)

    # Assert
    assert result.count() == expected


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
charge_links_dataset = [
    ("001-D01-001", "D01", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0))
]


# Shared
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
    spark: SparkSession, charges_with_prices: DataFrame, charge_links: DataFrame, expected: int
) -> None:
    # Arrange
    charges_with_prices = spark.createDataFrame(
        charges_with_prices, schema=charges_with_prices_schema
    )
    charge_links = spark.createDataFrame(charge_links, schema=charge_links_schema)

    # Act
    result = join_with_charge_links(charges_with_prices, charge_links)

    # Assert
    assert result.count() == expected


# Shared
DEFAULT_METERING_POINT_ID = "123"
ANOTHER_METERING_POINT_ID = "456"

charges_with_price_and_links_dataset_1 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        "P1D",
        "No",
        datetime(2020, 1, 15, 0, 0),
        Decimal("200.50"),
        DEFAULT_METERING_POINT_ID,
    )
]
charges_with_price_and_links_and_dataset_2 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        "P1D",
        "No",
        datetime(2020, 2, 1, 0, 0),
        Decimal("200.50"),
        DEFAULT_METERING_POINT_ID,
    )
]
charges_with_price_and_links_dataset_3 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        "P1D",
        "No",
        datetime(2020, 1, 1, 0, 0),
        Decimal("200.50"),
        DEFAULT_METERING_POINT_ID,
    )
]
charges_with_price_and_links_dataset_4 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        "P1D",
        "No",
        datetime(2020, 1, 15, 0, 0),
        Decimal("200.50"),
        ANOTHER_METERING_POINT_ID,
    )
]
metering_points_dataset = [
    (
        DEFAULT_METERING_POINT_ID,
        "E17",
        None,
        "D01",
        "1",
        "P1D",
        "2",
        "1",
        "1",
        "1",
        "1",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
    )
]


# Shared
@pytest.mark.parametrize(
    "charges_with_price_and_links,metering_points,expected",
    [
        (
            charges_with_price_and_links_dataset_1,
            metering_points_dataset,
            1,
        ),
        (
            charges_with_price_and_links_and_dataset_2,
            metering_points_dataset,
            0,
        ),
        (
            charges_with_price_and_links_dataset_3,
            metering_points_dataset,
            1,
        ),
        (
            charges_with_price_and_links_dataset_4,
            metering_points_dataset,
            0,
        ),
    ],
)
def test__join_with_metering_points__joins_on_metering_point_id_and_time_is_between_from_and_to_date(
    spark: SparkSession, charges_with_price_and_links: DataFrame, metering_points: DataFrame, expected: int
) -> None:
    # Arrange
    charges_with_price_and_links = spark.createDataFrame(
        charges_with_price_and_links,
        schema=charges_with_price_and_links_schema,
    )
    metering_points = spark.createDataFrame(
        metering_points, schema=metering_point_period_schema
    )

    # Act
    result = join_with_metering_points(
        charges_with_price_and_links, metering_points
    )

    # Assert
    assert result.count() == expected


time_series_dataset_1 = [
    (
        "D01",
        Decimal("10"),
        "D01",
        datetime(2020, 1, 15, 5, 0),
    ),
    (
        "D01",
        Decimal("10"),
        "D01",
        datetime(2020, 1, 15, 1, 0),
    ),
    (
        "D01",
        Decimal("10"),
        "D01",
        datetime(2020, 1, 15, 1, 30),
    ),
    (
        "D01",
        Decimal("10"),
        "D01",
        datetime(2020, 1, 16, 1, 0),
    ),
]


# Tariff only
@pytest.mark.parametrize(
    "time_series,resolution_duration,expected_count,expected_quantity",
    [
        (time_series_dataset_1, ChargeResolution.DAY.value, 2, 30),
        (time_series_dataset_1, ChargeResolution.HOUR.value, 3, 20),
    ],
)
def test__group_by_time_series_on_metering_point_id_and_resolution_and_sum_quantity(
    spark: SparkSession, time_series: DataFrame, resolution_duration: ChargeResolution, expected_count: int, expected_quantity: int
) -> None:
    # Arrange
    time_series = spark.createDataFrame(time_series, schema=time_series_point_schema)

    # Act
    result = group_by_time_series_on_metering_point_id_and_resolution_and_sum_quantity(
        time_series, resolution_duration
    )
    result = result.orderBy(
        col(Colname.quantity).desc()
    )  # orderby quantity desc to get highest quantity first

    # Assert
    assert result.count() == expected_count
    assert (
        result.collect()[0][Colname.quantity] == expected_quantity
    )  # expected highest quantity


grouped_time_series_dataset_1 = [
    (
        "D01",
        Decimal("10"),
        "D01",
        datetime(2020, 1, 15, 0, 0),
    )
]
charges_complete_dataset_1 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        "P1D",
        "No",
        datetime(2020, 1, 15, 0, 0),
        Decimal("200.50"),
        "D01",
        "1",
        "E17",
        "E22",
        "1",
    )
]


# Tariff only
@pytest.mark.parametrize(
    "charges_complete,grouped_time_series,expected",
    [(charges_complete_dataset_1, grouped_time_series_dataset_1, 1)],
)
def test__join_with_grouped_time_series__joins_on_metering_point_and_time(
    spark: SparkSession, charges_complete: DataFrame, grouped_time_series: DataFrame, expected: int
) -> None:
    # Arrange
    grouped_time_series = spark.createDataFrame(
        grouped_time_series, schema=time_series_point_schema
    )
    charges_complete = spark.createDataFrame(
        charges_complete, schema=charges_complete_schema
    )

    # Act
    result = join_with_grouped_time_series(charges_complete, grouped_time_series)

    # Assert
    assert result.count() == expected


def test__get_tariff_charges__(
    metering_point_period_factory: Callable[..., DataFrame],
    time_series_factory: Callable[..., DataFrame],
    charge_master_data_factory: Callable[..., DataFrame],
    charge_links_factory: Callable[..., DataFrame],
    charge_prices_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    from_date = datetime(2020, 1, 1, 0, 0)
    to_date = datetime(2020, 2, 1, 0, 0)
    observation_time = datetime(2020, 1, 2, 0, 0)
    charge_time = observation_time
    metering_point_period = metering_point_period_factory(from_date, to_date)
    time_series = time_series_factory(observation_time)
    charge_master_data = charge_master_data_factory(
        from_date,
        to_date,
        charge_type=ChargeType.TARIFF,
        charge_resolution=ChargeResolution.HOUR,
    )
    charge_links = charge_links_factory(from_date, to_date)
    charge_prices = charge_prices_factory(charge_time)

    # Act
    tariffs = get_tariff_charges(metering_point_period, time_series, charge_master_data, charge_links, charge_prices, ChargeResolution.HOUR)

    # Assert
    assert tariffs.count() == 1


def test__get_tariff_charges__when_charge_data_match_the_resolution__returns_empty_tariffs(
    metering_point_period_factory: Callable[..., DataFrame],
    time_series_factory: Callable[..., DataFrame],
    charge_master_data_factory: Callable[..., DataFrame],
    charge_links_factory: Callable[..., DataFrame],
    charge_prices_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    observation_time = datetime(2020, 1, 2, 0, 0)
    charge_time = observation_time
    metering_point_period = metering_point_period_factory(DEFAULT_FROM_DATE, DEFAULT_TO_DATE)
    time_series = time_series_factory(observation_time)
    charge_master_data = charge_master_data_factory(
        DEFAULT_FROM_DATE,
        DEFAULT_TO_DATE,
        charge_type=ChargeType.TARIFF,
        charge_resolution=ChargeResolution.DAY,
    )
    charge_links = charge_links_factory(DEFAULT_FROM_DATE, DEFAULT_TO_DATE)
    charge_prices = charge_prices_factory(charge_time)

    # Act
    tariffs = get_tariff_charges(metering_point_period, time_series, charge_master_data, charge_links, charge_prices, ChargeResolution.HOUR)

    # Assert
    assert tariffs.count() == 0


def test__get_tariff_charges__returns_expected_quantities(
    metering_point_period_factory: Callable[..., DataFrame],
    time_series_factory: Callable[..., DataFrame],
    charge_master_data_factory: Callable[..., DataFrame],
    charge_links_factory: Callable[..., DataFrame],
    charge_prices_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    metering_point_period = metering_point_period_factory(DEFAULT_FROM_DATE, DEFAULT_TO_DATE)
    first_hour_start = DEFAULT_FROM_DATE + timedelta(hours=1)
    second_hour_start = first_hour_start + timedelta(hours=1)
    third_hour_start = first_hour_start + timedelta(hours=2)
    time_series = (
        time_series_factory(first_hour_start + timedelta(minutes=45), quantity=Decimal(1))           # hour 1, quarter 4
        .union(time_series_factory(second_hour_start, quantity=Decimal(2)))                          # hour 2, quarter 1
        .union(time_series_factory(second_hour_start + timedelta(minutes=15), quantity=Decimal(3)))  # hour 2, quarter 2
        .union(time_series_factory(second_hour_start + timedelta(minutes=30), quantity=Decimal(4)))  # hour 2, quarter 3
        .union(time_series_factory(second_hour_start + timedelta(minutes=45), quantity=Decimal(5)))  # hour 2, quarter 4
        .union(time_series_factory(third_hour_start, quantity=Decimal(6)))                           # hour 3, quarter 1
    )
    # expected_quantities = [1, 14, 6]

    charge_master_data = charge_master_data_factory(
        DEFAULT_FROM_DATE,
        DEFAULT_TO_DATE,
        charge_type=ChargeType.TARIFF,
        charge_resolution=ChargeResolution.HOUR,
    )
    charge_links = charge_links_factory(DEFAULT_FROM_DATE, DEFAULT_TO_DATE)
    charge_prices = (
        charge_prices_factory(time=first_hour_start)           # hour 1
        .union(charge_prices_factory(time=second_hour_start))  # hour 2
        .union(charge_prices_factory(time=third_hour_start))   # hour 3
    )

    # Act
    tariffs = get_tariff_charges(metering_point_period, time_series, charge_master_data, charge_links, charge_prices, ChargeResolution.HOUR)

    tariffs.show()

    # Assert
    assert tariffs.count() == 3
