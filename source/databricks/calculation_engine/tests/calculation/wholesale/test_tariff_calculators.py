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

import uuid
from datetime import datetime, timedelta
from decimal import Decimal
from typing import Any

import pytest
from pyspark.sql import SparkSession

from package.calculation.wholesale.tariff_calculators import (
    calculate_tariff_price_per_ga_co_es,
)
from package.calculation.wholesale.tariff_calculators import (
    sum_within_month,
)
from package.codelists import (
    ChargeQuality,
    ChargeType,
    ChargeUnit,
    MeteringPointType,
    SettlementMethod,
    WholesaleResultResolution,
)
from package.constants import Colname
import tests.calculation.wholesale.prepared_tariffs_factory as factory


def test__calculate_tariff_price_per_ga_co_es__returns_empty_df_when_input_df_is_empty(
    spark: SparkSession,
) -> None:
    # Arrange
    prepared_tariff = factory.create(spark, [])

    # Act
    actual = calculate_tariff_price_per_ga_co_es(prepared_tariff)

    # Assert
    assert actual.count() == 0


def test__calculate_tariff_price_per_ga_co_es__returns_df_with_correct_columns(
    spark: SparkSession,
) -> None:
    # Arrange
    prepared_tariff = factory.create(spark, [])

    # Act
    actual = calculate_tariff_price_per_ga_co_es(prepared_tariff)

    # Assert
    assert Colname.energy_supplier_id in actual.columns
    assert Colname.grid_area in actual.columns
    assert Colname.charge_time in actual.columns
    assert Colname.metering_point_type in actual.columns
    assert Colname.settlement_method in actual.columns
    assert Colname.charge_key in actual.columns
    assert Colname.charge_code in actual.columns
    assert Colname.charge_type in actual.columns
    assert Colname.charge_owner in actual.columns
    assert Colname.charge_tax in actual.columns
    assert Colname.resolution in actual.columns
    assert Colname.charge_price in actual.columns
    assert Colname.total_quantity in actual.columns
    assert Colname.total_amount in actual.columns
    assert Colname.unit in actual.columns
    assert Colname.qualities in actual.columns


def test__calculate_tariff_price_per_ga_co_es__returns_df_with_expected_values(
    spark: SparkSession,
) -> None:
    # Arrange: 3 rows that should all be aggregated into a single row
    CHARGE_KEY = "charge-key"
    rows = [
        factory.create_row(metering_point_id="1", charge_key=CHARGE_KEY),
        factory.create_row(metering_point_id="2", charge_key=CHARGE_KEY),
        factory.create_row(metering_point_id="3", charge_key=CHARGE_KEY),
    ]

    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(prepared_tariffs)

    # Assert
    assert actual.count() == 1
    actual_row = actual.collect()[0]

    assert (
        actual_row[Colname.energy_supplier_id]
        == factory.DefaultValues.ENERGY_SUPPLIER_ID
    )
    assert actual_row[Colname.grid_area] == factory.DefaultValues.GRID_AREA
    assert actual_row[Colname.charge_time] == factory.DefaultValues.CHARGE_TIME_HOUR_0
    assert (
        actual_row[Colname.metering_point_type]
        == factory.DefaultValues.METERING_POINT_TYPE.value
    )
    assert (
        actual_row[Colname.settlement_method]
        == factory.DefaultValues.SETTLEMENT_METHOD.value
    )
    assert actual_row[Colname.charge_key] == CHARGE_KEY
    assert actual_row[Colname.charge_code] == factory.DefaultValues.CHARGE_CODE
    assert actual_row[Colname.charge_type] == ChargeType.TARIFF.value
    assert actual_row[Colname.charge_owner] == factory.DefaultValues.CHARGE_OWNER
    assert actual_row[Colname.charge_tax] == factory.DefaultValues.CHARGE_TAX
    assert actual_row[Colname.resolution] == WholesaleResultResolution.HOUR.value
    assert actual_row[Colname.charge_price] == factory.DefaultValues.CHARGE_PRICE
    assert actual_row[Colname.total_quantity] == 3 * factory.DefaultValues.QUANTITY
    assert actual_row[Colname.total_amount] == Decimal(
        "6.030015"
    )  # 3 * DEFAULT_CHARGE_PRICE * DEFAULT_QUANTITY rounded to 6 decimals
    assert actual_row[Colname.unit] == ChargeUnit.KWH.value
    assert actual_row[Colname.qualities] == [ChargeQuality.CALCULATED.value]


def test__calculate_tariff_price_per_ga_co_es__returns_all_qualities(
    spark: SparkSession,
) -> None:
    # Arrange: A number of rows with different qualities. All rows should be aggregated into a single row containing all the qualities.
    expected_qualities = [
        ChargeQuality.CALCULATED,
        ChargeQuality.ESTIMATED,
        ChargeQuality.MEASURED,
    ]
    expected_quality_values = [quality.value for quality in expected_qualities]

    rows = [
        factory.create_row(metering_point_id=str(uuid.uuid4()), quality=quality)
        for quality in expected_qualities
    ]
    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(prepared_tariffs)

    # Assert
    actual_row = actual.collect()[0]
    actual_qualities = actual_row[Colname.qualities]
    assert set(actual_qualities) == set(expected_quality_values)


@pytest.mark.parametrize(
    "column_name, value, other_value",
    [
        ("grid_area", "1", "2"),
        ("energy_supplier_id", "1", "2"),
        (
            "metering_point_type",
            MeteringPointType.CONSUMPTION,
            MeteringPointType.PRODUCTION,
        ),
        ("settlement_method", SettlementMethod.FLEX, SettlementMethod.NON_PROFILED),
        ("charge_key", "1", "2"),
        ("charge_code", "1", "2"),
        ("charge_owner", "1", "2"),
        (
            "charge_time",
            factory.DefaultValues.CHARGE_TIME_HOUR_0,
            factory.DefaultValues.CHARGE_TIME_HOUR_0 + timedelta(hours=1),
        ),
    ],
)
def test__calculate_tariff_price_per_ga_co_es__does_not_aggregate_across_group_splitting_columns(
    spark: SparkSession, column_name: str, value: Any, other_value: Any
) -> None:
    """
    Charge type, is-tax, and resolution should never have different values.
    This is by code/module design but it's not verified in the production code as it would have a significant performance impact.
    """

    # Arrange
    rows = [
        factory.create_row(**{column_name: value}),
        factory.create_row(**{column_name: other_value}),
    ]
    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(prepared_tariffs)

    # Assert
    assert actual.count() == 2


def test__calculate_tariff_price_per_ga_co_es__when_settlement_method_is_null__returns_result(
    spark: SparkSession,
) -> None:
    """
    Settlement method being null is a permutation that should be tested.
    This is the case for all but consumption metering points.
    This test tests for one of these examples.
    """

    # Arrange
    rows = [
        factory.create_row(
            metering_point_type=MeteringPointType.PRODUCTION, settlement_method=None
        )
    ]
    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(prepared_tariffs)

    # Assert
    assert actual.count() == 1


@pytest.mark.parametrize(
    "column_name, expected_scale",
    [
        (Colname.total_amount, 6),
        (Colname.total_quantity, 3),
        (Colname.charge_price, 6),
    ],
)
def test__calculate_tariff_price_per_ga_co_es__returns_df_with_expected_scale(
    spark: SparkSession, column_name: str, expected_scale: int
) -> None:
    # Arrange
    rows = [factory.create_row()]
    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(prepared_tariffs)

    # Assert
    assert actual.schema[column_name].dataType.scale == expected_scale


def test__calculate_tariff_price_per_ga_co_es__when_production__returns_df_with_expected_precision(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [factory.create_row()]
    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(prepared_tariffs)

    # Assert
    assert actual.schema[Colname.total_amount].dataType.precision >= 18


@pytest.mark.parametrize(
    "charge_price, quantity, expected_total_amount",
    [
        (Decimal("0.000001"), Decimal("0.499"), Decimal("0.000000")),
        (Decimal("0.000001"), Decimal("0.500"), Decimal("0.000001")),
        (Decimal("0.000499"), Decimal("0.001"), Decimal("0.000000")),
        (Decimal("0.000500"), Decimal("0.001"), Decimal("0.000001")),
    ],
)
def test__calculate_tariff_price_per_ga_co_es__rounds_total_amount_correctly(
    spark: SparkSession,
    charge_price: Decimal,
    quantity: Decimal,
    expected_total_amount: Decimal,
) -> None:
    # Arrange
    rows = [factory.create_row(charge_price=charge_price, quantity=quantity)]
    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(prepared_tariffs)

    # Assert
    actual_amount = actual.collect()[0][Colname.total_amount]
    assert actual_amount == expected_total_amount


def test__sum_within_month__sums_amount_per_month(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        factory.create_row(charge_time=datetime(2020, 1, 1, 1)),
        factory.create_row(charge_time=datetime(2020, 1, 1, 0)),
    ]
    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        calculate_tariff_price_per_ga_co_es(prepared_tariffs),
        factory.DefaultValues.PERIOD_START_DATETIME,
    )

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("4.020010")
    assert actual.count() == 1


def test__sum_within_month__sums_across_metering_point_types(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        factory.create_row(metering_point_type=MeteringPointType.PRODUCTION),
        factory.create_row(metering_point_type=MeteringPointType.CONSUMPTION),
    ]
    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        calculate_tariff_price_per_ga_co_es(prepared_tariffs),
        factory.DefaultValues.PERIOD_START_DATETIME,
    )

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("4.020010")
    assert actual.count() == 1


def test__sum_within_month__joins_qualities(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        factory.create_row(quality=ChargeQuality.CALCULATED),
        factory.create_row(quality=ChargeQuality.ESTIMATED),
    ]
    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        calculate_tariff_price_per_ga_co_es(prepared_tariffs),
        factory.DefaultValues.PERIOD_START_DATETIME,
    )

    # Assert
    assert actual.collect()[0][Colname.qualities] == ["calculated", "estimated"]
    assert actual.collect()[0][Colname.total_amount] == Decimal("4.020010")
    assert actual.count() == 1


def test__sum_within_month__groups_by_local_time_months(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        factory.create_row(charge_time=datetime(2020, 1, 1, 0)),
        factory.create_row(charge_time=datetime(2019, 12, 31, 23)),
    ]
    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        calculate_tariff_price_per_ga_co_es(prepared_tariffs),
        factory.DefaultValues.PERIOD_START_DATETIME,
    )

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("4.020010")
    assert actual.collect()[0][Colname.charge_time] == datetime(2019, 12, 31, 23)
    assert actual.count() == 1


def test__sum_within_month__charge_time_always_start_of_month(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        factory.create_row(charge_time=datetime(2020, 1, 3, 0)),
    ]
    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        calculate_tariff_price_per_ga_co_es(prepared_tariffs),
        factory.DefaultValues.PERIOD_START_DATETIME,
    )

    # Assert
    assert actual.collect()[0][Colname.charge_time] == datetime(2019, 12, 31, 23)


def test__sum_within_month__sums_quantity_per_month(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        factory.create_row(quantity=Decimal("1.111")),
        factory.create_row(quantity=Decimal("1.111")),
    ]
    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        calculate_tariff_price_per_ga_co_es(prepared_tariffs),
        factory.DefaultValues.PERIOD_START_DATETIME,
    )

    # Assert
    assert actual.collect()[0][Colname.total_quantity] == Decimal("2.222")
    assert actual.count() == 1


def test__sum_within_month__sets_charge_price_to_none(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        factory.create_row(
            charge_time=datetime(2020, 1, 1, 0), charge_price=Decimal("1.111111")
        ),
        factory.create_row(
            charge_time=datetime(2020, 1, 1, 1), charge_price=Decimal("1.111111")
        ),
    ]
    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        calculate_tariff_price_per_ga_co_es(prepared_tariffs),
        factory.DefaultValues.PERIOD_START_DATETIME,
    )

    # Assert
    assert actual.collect()[0][Colname.charge_price] is None
    assert actual.count() == 1


def test__calculate_tariff_price_per_ga_co_es__when_charge_price_is_null__returns_total_amount_as_none(
    spark: SparkSession,
) -> None:
    """
    This is a test for a case where the charge price is none.
    When charge_price is null, the total_amount should also be none.
    """

    # Arrange
    rows = [
        factory.create_row(charge_price=None, quantity=Decimal("2.000000")),
    ]
    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(prepared_tariffs)

    # Assert
    assert actual.collect()[0][Colname.total_amount] is None


def test__sum_within_month__when_all_charge_prices_are_none__sums_charge_price_and_total_amount_per_month_to_none(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        factory.create_row(charge_price=None),
        factory.create_row(charge_price=None),
    ]
    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        calculate_tariff_price_per_ga_co_es(prepared_tariffs),
        factory.DefaultValues.PERIOD_START_DATETIME,
    )

    # Assert
    assert actual.collect()[0][Colname.total_amount] is None
    assert actual.collect()[0][Colname.charge_price] is None
    assert actual.count() == 1


def test__sum_within_month__when_one_tariff_has_charge_price_none__sums_charge_price_and_total_amount_per_month_to_expected_value(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        factory.create_row(charge_price=None),
        factory.create_row(
            charge_price=Decimal("2.000000"), quantity=Decimal("3.000000")
        ),
    ]
    prepared_tariffs = factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        calculate_tariff_price_per_ga_co_es(prepared_tariffs),
        factory.DefaultValues.PERIOD_START_DATETIME,
    )

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("6.000000")
    assert actual.count() == 1
