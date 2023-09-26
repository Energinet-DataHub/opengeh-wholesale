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
from pyspark.sql.types import (
    DecimalType,
    StringType,
    StructField,
    StructType,
    TimestampType,
)
from typing import Callable
from package.calculation.wholesale.wholesale_initializer import (
    _join_with_metering_points,
    _explode_subscription,
    _get_charges_based_on_resolution,
    get_tariff_charges,
    _group_by_time_series_on_metering_point_id_and_resolution_and_sum_quantity,
    _join_with_grouped_time_series,
    _get_charges_based_on_charge_type,
)
from package.codelists import (
    ChargeQuality,
    ChargeType,
    ChargeResolution,
    MeteringPointType,
    SettlementMethod,
)
from package.calculation.wholesale.schemas.charges_schema import (
    charges_schema,
)
from package.calculation.wholesale.charges_reader import _create_charges_df
from package.calculation_input.schemas import (
    time_series_point_schema,
    metering_point_period_schema,
)
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
DEFAULT_TIME = datetime(2020, 1, 1, 0, 0)

charges_dataset = [
    (
        "001-D01-001",
        "001",
        ChargeType.TARIFF.value,
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
        ChargeType.TARIFF.value,
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
        ChargeType.TARIFF.value,
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
        ChargeType.TARIFF.value,
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
    spark: SparkSession,
    charges: DataFrame,
    resolution_duration: ChargeResolution,
    expected: int,
) -> None:
    # Arrange
    charges = spark.createDataFrame(charges, schema=charges_schema)

    # Act
    result = _get_charges_based_on_resolution(charges, resolution_duration)

    # Assert
    assert result.count() == expected


charges_dataset = [
    (
        "001-D01-001",
        "001",
        ChargeType.TARIFF.value,
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
        ChargeType.TARIFF.value,
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
        ChargeType.SUBSCRIPTION.value,
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
        ChargeType.SUBSCRIPTION.value,
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
        ChargeType.FEE.value,
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
        ChargeType.TARIFF.value,
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
    spark: SparkSession, charges: DataFrame, charge_type: ChargeType, expected: int
) -> None:
    # Arrange
    charges = spark.createDataFrame(charges, schema=charges_schema)

    # Act
    result = _get_charges_based_on_charge_type(charges, charge_type)

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
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
        datetime(2020, 1, 15, 0, 0),
        Decimal("200.50"),
        DEFAULT_METERING_POINT_ID,
    )
]
charges_with_price_and_links_dataset_2 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        "P1D",
        "No",
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
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
        datetime(2020, 2, 2, 0, 0),
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
        datetime(2020, 1, 1, 0, 0),
        datetime(2020, 2, 1, 0, 0),
        datetime(2020, 1, 15, 0, 0),
        Decimal("200.50"),
        ANOTHER_METERING_POINT_ID,
    )
]
metering_points_dataset = [
    (
        DEFAULT_METERING_POINT_ID,
        MeteringPointType.CONSUMPTION.value,
        None,
        SettlementMethod.FLEX.value,
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


explode_charges_with_price_and_links_dataset_1 = [
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
        DEFAULT_METERING_POINT_ID,
    )
]
explode_charges_with_price_and_links_dataset_2 = [
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
        DEFAULT_METERING_POINT_ID,
    )
]
explode_charges_with_price_and_links_dataset_3 = [
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
        DEFAULT_METERING_POINT_ID,
    )
]
explode_charges_with_price_and_links_dataset_4 = [
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
        ANOTHER_METERING_POINT_ID,
    )
]


# Subscription only
@pytest.mark.parametrize(
    "charges_df,expected",
    [
        (explode_charges_with_price_and_links_dataset_1, 31),
        (explode_charges_with_price_and_links_dataset_2, 0),
        (explode_charges_with_price_and_links_dataset_3, 2),
        (explode_charges_with_price_and_links_dataset_4, 0),
    ],
)
def test__explode_subscription__explodes_into_rows_based_on_number_of_days_between_from_and_to_date(
    spark: SparkSession, charges_df: DataFrame, expected: int
) -> None:
    # Arrange
    charges_df = spark.createDataFrame(
        charges_df, schema=charges_with_price_and_links_schema
    )

    # Act
    result = _explode_subscription(charges_df)

    # Assert
    assert result.count() == expected


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
            charges_with_price_and_links_dataset_2,
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
    spark: SparkSession,
    charges_with_price_and_links: DataFrame,
    metering_points: DataFrame,
    expected: int,
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
    result = _join_with_metering_points(charges_with_price_and_links, metering_points)

    # Assert
    assert result.count() == expected


time_series_dataset_1 = [
    (
        "D01",
        Decimal("10"),
        [ChargeQuality.CALCULATED.value, ChargeQuality.ESTIMATED.value],
        datetime(2020, 1, 15, 5, 0),
    ),
    (
        "D01",
        Decimal("10"),
        [ChargeQuality.CALCULATED.value, ChargeQuality.ESTIMATED.value],
        datetime(2020, 1, 15, 1, 0),
    ),
    (
        "D01",
        Decimal("10"),
        [ChargeQuality.CALCULATED.value, ChargeQuality.ESTIMATED.value],
        datetime(2020, 1, 15, 1, 30),
    ),
    (
        "D01",
        Decimal("10"),
        [ChargeQuality.CALCULATED.value, ChargeQuality.ESTIMATED.value],
        datetime(2020, 1, 16, 1, 0),
    ),
]


# Tariff only
@pytest.mark.parametrize(
    "time_series,resolution_duration,expected_count,expected_quantity",
    [
        (time_series_dataset_1, ChargeResolution.DAY, 2, 30),
        (time_series_dataset_1, ChargeResolution.HOUR, 3, 20),
    ],
)
def test__group_by_time_series_on_metering_point_id_and_resolution_and_sum_quantity(
    spark: SparkSession,
    time_series: DataFrame,
    resolution_duration: ChargeResolution,
    expected_count: int,
    expected_quantity: int,
) -> None:
    # Arrange
    time_series = spark.createDataFrame(time_series, schema=time_series_point_schema)

    # Act
    result = _group_by_time_series_on_metering_point_id_and_resolution_and_sum_quantity(
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
        [ChargeQuality.CALCULATED.value, ChargeQuality.ESTIMATED.value],
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
        MeteringPointType.CONSUMPTION.value,
        SettlementMethod.NON_PROFILED.value,
        "1",
    )
]


grouped_time_series_point_schema = StructType(
    [
        StructField(Colname.metering_point_id, StringType(), False),
        StructField(Colname.quantity, DecimalType(18, 6), True),
        StructField(Colname.qualities, StringType(), False),
        StructField(Colname.observation_time, TimestampType(), False),
    ]
)


# Tariff only
@pytest.mark.parametrize(
    "charges_complete,grouped_time_series,expected",
    [(charges_complete_dataset_1, grouped_time_series_dataset_1, 1)],
)
def test__join_with_grouped_time_series__joins_on_metering_point_and_time(
    spark: SparkSession,
    charges_complete: DataFrame,
    grouped_time_series: DataFrame,
    expected: int,
) -> None:
    # Arrange
    grouped_time_series = spark.createDataFrame(
        grouped_time_series, schema=grouped_time_series_point_schema
    )
    charges_complete = spark.createDataFrame(
        charges_complete, schema=charges_complete_schema
    )

    # Act
    result = _join_with_grouped_time_series(charges_complete, grouped_time_series)

    # Assert
    assert result.count() == expected


@pytest.fixture(scope="session")
def default_time_series_point(
    time_series_factory: Callable[..., DataFrame]
) -> DataFrame:
    return time_series_factory(DEFAULT_TIME)


@pytest.fixture(scope="session")
def default_metering_point_period(
    metering_point_period_factory: Callable[..., DataFrame]
) -> DataFrame:
    return metering_point_period_factory(DEFAULT_FROM_DATE, DEFAULT_TO_DATE)


@pytest.fixture(scope="session")
def default_charge_master_data(
    charge_master_data_factory: Callable[..., DataFrame]
) -> DataFrame:
    return (
        charge_master_data_factory(
            DEFAULT_FROM_DATE,
            DEFAULT_TO_DATE,
            charge_type=ChargeType.TARIFF.value,
            charge_resolution=ChargeResolution.HOUR.value,
        )
        .union(
            charge_master_data_factory(
                DEFAULT_FROM_DATE,
                DEFAULT_TO_DATE,
                charge_type=ChargeType.FEE.value,
                charge_resolution=ChargeResolution.HOUR.value,
            )
        )
        .union(
            charge_master_data_factory(
                DEFAULT_FROM_DATE,
                DEFAULT_TO_DATE,
                charge_type=ChargeType.SUBSCRIPTION.value,
                charge_resolution=ChargeResolution.HOUR.value,
            )
        )
    )


@pytest.fixture(scope="session")
def default_charge_links(charge_links_factory: Callable[..., DataFrame]) -> DataFrame:
    return charge_links_factory(DEFAULT_FROM_DATE, DEFAULT_TO_DATE)


@pytest.fixture(scope="session")
def default_charge_price_point(
    charge_prices_factory: Callable[..., DataFrame]
) -> DataFrame:
    return charge_prices_factory(time=DEFAULT_TIME)


def test__get_tariff_charges__when_no_charge_data_match_the_resolution__returns_empty_tariffs(
    default_metering_point_period: DataFrame,
    default_time_series_point: DataFrame,
    default_charge_links: DataFrame,
    default_charge_price_point: DataFrame,
    charge_master_data_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    charge_master_data = charge_master_data_factory(
        DEFAULT_FROM_DATE,
        DEFAULT_TO_DATE,
        charge_type=ChargeType.TARIFF.value,
        charge_resolution=ChargeResolution.DAY.value,
    )

    charges_df = _create_charges_df(
        charge_master_data, default_charge_links, default_charge_price_point
    )

    # Act
    tariffs = get_tariff_charges(
        default_metering_point_period,
        default_time_series_point,
        charges_df,
        ChargeResolution.HOUR,
    )

    # Assert
    assert tariffs.count() == 0


def test__get_tariff_charges__returns_expected_quantities(
    default_charge_master_data: DataFrame,
    default_metering_point_period: DataFrame,
    default_charge_links: DataFrame,
    time_series_factory: Callable[..., DataFrame],
    charge_prices_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    first_hour_start = DEFAULT_FROM_DATE + timedelta(hours=1)
    second_hour_start = first_hour_start + timedelta(hours=1)
    third_hour_start = first_hour_start + timedelta(hours=2)
    time_series = (
        time_series_factory(
            first_hour_start + timedelta(minutes=45), quantity=Decimal(1)
        )  # hour 1, quarter 4
        .union(
            time_series_factory(second_hour_start, quantity=Decimal(2))
        )  # hour 2, quarter 1
        .union(
            time_series_factory(
                second_hour_start + timedelta(minutes=15), quantity=Decimal(3)
            )
        )  # hour 2, quarter 2
        .union(
            time_series_factory(
                second_hour_start + timedelta(minutes=30), quantity=Decimal(4)
            )
        )  # hour 2, quarter 3
        .union(
            time_series_factory(
                second_hour_start + timedelta(minutes=45), quantity=Decimal(5)
            )
        )  # hour 2, quarter 4
        .union(
            time_series_factory(third_hour_start, quantity=Decimal(6))
        )  # hour 3, quarter 1
    )
    expected_quantities = [1, 14, 6]

    charge_prices = (
        charge_prices_factory(time=first_hour_start)  # hour 1
        .union(charge_prices_factory(time=second_hour_start))  # hour 2
        .union(charge_prices_factory(time=third_hour_start))  # hour 3
    )

    charges_df = _create_charges_df(
        default_charge_master_data, default_charge_links, charge_prices
    )

    # Act
    tariffs = get_tariff_charges(
        default_metering_point_period,
        time_series,
        charges_df,
        ChargeResolution.HOUR,
    )

    # Assert
    assert tariffs.count() == len(expected_quantities)
    assert tariffs.collect()[0][Colname.quantity] == expected_quantities[0]
    assert tariffs.collect()[1][Colname.quantity] == expected_quantities[1]
    assert tariffs.collect()[2][Colname.quantity] == expected_quantities[2]


def _create_overlapping_hour_tariffs(
    charge_master_data_factory: Callable[..., DataFrame],
    charge_links_factory: Callable[..., DataFrame],
    charge_prices_factory: Callable[..., DataFrame],
    charge_key_1: str,
    charge_key_2: str,
) -> tuple[DataFrame, DataFrame, DataFrame]:
    charge_master_data = charge_master_data_factory(
        DEFAULT_FROM_DATE,
        DEFAULT_TO_DATE,
        charge_key=charge_key_1,
        charge_type=ChargeType.TARIFF.value,
        charge_resolution=ChargeResolution.HOUR.value,
    ).union(
        charge_master_data_factory(
            DEFAULT_FROM_DATE,
            DEFAULT_TO_DATE,
            charge_key=charge_key_2,
            charge_type=ChargeType.TARIFF.value,
            charge_resolution=ChargeResolution.HOUR.value,
        )
    )
    charge_links = charge_links_factory(
        DEFAULT_FROM_DATE,
        DEFAULT_TO_DATE,
        charge_key=charge_key_1,
    ).union(
        charge_links_factory(
            DEFAULT_FROM_DATE,
            DEFAULT_TO_DATE,
            charge_key=charge_key_2,
        )
    )
    charge_prices = charge_prices_factory(DEFAULT_TIME, charge_key_1).union(
        charge_prices_factory(DEFAULT_TIME, charge_key_2)
    )

    return charge_master_data, charge_links, charge_prices


def test__get_tariff_charges__when_two_tariff_overlap__returns_both_tariffs(
    default_metering_point_period: DataFrame,
    default_time_series_point: DataFrame,
    charge_links_factory: Callable[..., DataFrame],
    charge_prices_factory: Callable[..., DataFrame],
    charge_master_data_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    charge_key_1 = "charge_key_1"
    charge_key_2 = "charge_key_2"
    charge_master_data, charge_links, charge_prices = _create_overlapping_hour_tariffs(
        charge_master_data_factory,
        charge_links_factory,
        charge_prices_factory,
        charge_key_1,
        charge_key_2,
    )

    charges_df = _create_charges_df(charge_master_data, charge_links, charge_prices)

    # Act
    tariffs_df = get_tariff_charges(
        default_metering_point_period,
        default_time_series_point,
        charges_df,
        ChargeResolution.HOUR,
    )

    # Assert
    assert tariffs_df.count() == 2
    tariffs = tariffs_df.sort(Colname.charge_key).collect()
    assert tariffs[0][Colname.charge_key] == charge_key_1
    assert tariffs[1][Colname.charge_key] == charge_key_2
