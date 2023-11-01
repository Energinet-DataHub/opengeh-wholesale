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
from datetime import datetime, timedelta
import uuid

from pyspark import Row
from pyspark.sql import SparkSession
import pytest
from typing import Any, Union

from package.calculation.wholesale.schemas.tariffs_schema import tariff_schema
from package.codelists import (
    ChargeQuality,
    ChargeType,
    ChargeUnit,
    MeteringPointType,
    SettlementMethod,
    WholesaleResultResolution,
)
from package.calculation.wholesale.tariff_calculators import (
    calculate_tariff_price_per_ga_co_es,
)
from package.calculation.wholesale.tariff_calculators import (
    sum_within_month,
)
from package.constants import Colname


DEFAULT_GRID_AREA = "543"
DEFAULT_CHARGE_CODE = "4000"
DEFAULT_CHARGE_OWNER = "001"
DEFAULT_CHARGE_TAX = True
DEFAULT_CHARGE_TIME_HOUR_0 = datetime(2020, 1, 1, 0)
DEFAULT_CHARGE_PRICE = Decimal("2.000005")
DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"
DEFAULT_METERING_POINT_ID = "123456789012345678901234567"
DEFAULT_METERING_POINT_TYPE = MeteringPointType.CONSUMPTION
DEFAULT_SETTLEMENT_METHOD = SettlementMethod.FLEX
DEFAULT_QUANTITY = Decimal("1.005")
DEFAULT_QUALITY = ChargeQuality.CALCULATED
DEFAULT_PERIOD_START_DATETIME = datetime(2019, 12, 31, 23)


def _create_tariff_hour_row(
    charge_key: Union[str, None] = None,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_time: datetime = DEFAULT_CHARGE_TIME_HOUR_0,
    charge_price: Decimal = DEFAULT_CHARGE_PRICE,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    metering_point_type: MeteringPointType = DEFAULT_METERING_POINT_TYPE,
    settlement_method: Union[SettlementMethod, None] = DEFAULT_SETTLEMENT_METHOD,
    grid_area: str = DEFAULT_GRID_AREA,
    quantity: Decimal = DEFAULT_QUANTITY,
    quality: ChargeQuality = DEFAULT_QUALITY,
) -> Row:
    row = {
        Colname.charge_key: charge_key
        or f"{charge_code}-{ChargeType.TARIFF.value}-{charge_owner}",
        Colname.charge_code: charge_code,
        Colname.charge_type: ChargeType.TARIFF.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_tax: DEFAULT_CHARGE_TAX,
        Colname.charge_time: charge_time,
        Colname.charge_price: charge_price,
        Colname.metering_point_id: metering_point_id,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.metering_point_type: metering_point_type.value,
        Colname.settlement_method: settlement_method.value
        if settlement_method
        else None,
        Colname.grid_area: grid_area,
        Colname.quantity: quantity,
        Colname.qualities: [quality.value],
        Colname.wholesale_result_resolution: WholesaleResultResolution.HOUR.value,
    }

    return Row(**row)


def test__calculate_tariff_price_per_ga_co_es__raises_value_error_when_input_df_has_wrong_schema(
    spark: SparkSession,
) -> None:
    # Arrange
    tariffs = spark.createDataFrame(data=[{"Hello": "World"}])

    # Act
    with pytest.raises(AssertionError) as excinfo:
        calculate_tariff_price_per_ga_co_es(tariffs)

    # Assert
    assert "Schema mismatch" in str(excinfo.value)


def test__calculate_tariff_price_per_ga_co_es__returns_empty_df_when_input_df_is_empty(
    spark: SparkSession,
) -> None:
    # Arrange
    tariffs = spark.createDataFrame(data=[], schema=tariff_schema)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(tariffs)

    # Assert
    assert actual.count() == 0


def test__calculate_tariff_price_per_ga_co_es__returns_df_with_correct_columns(
    spark: SparkSession,
) -> None:
    # Arrange
    tariffs = spark.createDataFrame(data=[], schema=tariff_schema)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(tariffs)

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
    assert Colname.wholesale_result_resolution in actual.columns
    assert Colname.charge_price in actual.columns
    assert Colname.total_quantity in actual.columns
    assert Colname.charge_count in actual.columns
    assert Colname.total_amount in actual.columns
    assert Colname.unit in actual.columns
    assert Colname.qualities in actual.columns


def test__calculate_tariff_price_per_ga_co_es__returns_df_with_expected_values(
    spark: SparkSession,
) -> None:
    # Arrange: 3 rows that should all be aggregated into a single row
    CHARGE_KEY = "charge-key"
    rows = [
        _create_tariff_hour_row(metering_point_id="1", charge_key=CHARGE_KEY),
        _create_tariff_hour_row(metering_point_id="2", charge_key=CHARGE_KEY),
        _create_tariff_hour_row(metering_point_id="3", charge_key=CHARGE_KEY),
    ]

    tariffs = spark.createDataFrame(data=rows, schema=tariff_schema)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(tariffs)

    # Assert
    assert actual.count() == 1
    actual_row = actual.collect()[0]

    assert actual_row[Colname.energy_supplier_id] == DEFAULT_ENERGY_SUPPLIER_ID
    assert actual_row[Colname.grid_area] == DEFAULT_GRID_AREA
    assert actual_row[Colname.charge_time] == DEFAULT_CHARGE_TIME_HOUR_0
    assert actual_row[Colname.metering_point_type] == DEFAULT_METERING_POINT_TYPE.value
    assert actual_row[Colname.settlement_method] == DEFAULT_SETTLEMENT_METHOD.value
    assert actual_row[Colname.charge_key] == CHARGE_KEY
    assert actual_row[Colname.charge_code] == DEFAULT_CHARGE_CODE
    assert actual_row[Colname.charge_type] == ChargeType.TARIFF.value
    assert actual_row[Colname.charge_owner] == DEFAULT_CHARGE_OWNER
    assert actual_row[Colname.charge_tax] == DEFAULT_CHARGE_TAX
    assert (
        actual_row[Colname.wholesale_result_resolution]
        == WholesaleResultResolution.HOUR.value
    )
    assert actual_row[Colname.charge_price] == DEFAULT_CHARGE_PRICE
    assert actual_row[Colname.total_quantity] == 3 * DEFAULT_QUANTITY
    assert actual_row[Colname.charge_count] == 3
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
        _create_tariff_hour_row(metering_point_id=str(uuid.uuid4()), quality=quality)
        for quality in expected_qualities
    ]
    tariffs = spark.createDataFrame(data=rows, schema=tariff_schema)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(tariffs)

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
            DEFAULT_CHARGE_TIME_HOUR_0,
            DEFAULT_CHARGE_TIME_HOUR_0 + timedelta(hours=1),
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
        _create_tariff_hour_row(**{column_name: value}),
        _create_tariff_hour_row(**{column_name: other_value}),
    ]
    tariffs = spark.createDataFrame(data=rows, schema=tariff_schema)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(tariffs)

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
        _create_tariff_hour_row(
            metering_point_type=MeteringPointType.PRODUCTION, settlement_method=None
        )
    ]
    tariffs = spark.createDataFrame(data=rows, schema=tariff_schema)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(tariffs)

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
    rows = [_create_tariff_hour_row()]
    tariffs = spark.createDataFrame(data=rows, schema=tariff_schema)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(tariffs)

    # Assert
    assert actual.schema[column_name].dataType.scale == expected_scale


def test__calculate_tariff_price_per_ga_co_es__when_production__returns_df_with_expected_precision(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [_create_tariff_hour_row()]
    tariffs = spark.createDataFrame(data=rows, schema=tariff_schema)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(tariffs)

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
    rows = [_create_tariff_hour_row(charge_price=charge_price, quantity=quantity)]
    tariffs = spark.createDataFrame(data=rows, schema=tariff_schema)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(tariffs)

    # Assert
    actual_amount = actual.collect()[0][Colname.total_amount]
    assert actual_amount == expected_total_amount


def test__sum_within_month__sums_amount_per_month(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        _create_tariff_hour_row(charge_time=datetime(2020, 1, 1, 1)),
        _create_tariff_hour_row(charge_time=datetime(2020, 1, 1, 0)),
    ]
    tariffs = spark.createDataFrame(data=rows, schema=tariff_schema)

    # Act
    actual = sum_within_month(
        calculate_tariff_price_per_ga_co_es(tariffs),
        DEFAULT_PERIOD_START_DATETIME,
    )

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("4.020010")
    assert actual.count() == 1


def test__sum_within_month__sums_across_metering_point_types(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        _create_tariff_hour_row(metering_point_type=MeteringPointType.PRODUCTION),
        _create_tariff_hour_row(metering_point_type=MeteringPointType.CONSUMPTION),
    ]
    tariffs = spark.createDataFrame(data=rows, schema=tariff_schema)

    # Act
    actual = sum_within_month(
        calculate_tariff_price_per_ga_co_es(tariffs),
        DEFAULT_PERIOD_START_DATETIME,
    )

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("4.020010")
    assert actual.count() == 1


def test__sum_within_month__joins_qualities(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        _create_tariff_hour_row(quality=ChargeQuality.CALCULATED),
        _create_tariff_hour_row(quality=ChargeQuality.ESTIMATED),
    ]
    tariffs = spark.createDataFrame(data=rows, schema=tariff_schema)

    # Act
    actual = sum_within_month(
        calculate_tariff_price_per_ga_co_es(tariffs),
        DEFAULT_PERIOD_START_DATETIME,
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
        _create_tariff_hour_row(charge_time=datetime(2020, 1, 1, 0)),
        _create_tariff_hour_row(charge_time=datetime(2019, 12, 31, 23)),
    ]
    tariffs = spark.createDataFrame(data=rows, schema=tariff_schema)

    # Act
    actual = sum_within_month(
        calculate_tariff_price_per_ga_co_es(tariffs),
        DEFAULT_PERIOD_START_DATETIME,
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
        _create_tariff_hour_row(charge_time=datetime(2020, 1, 3, 0)),
    ]
    tariffs = spark.createDataFrame(data=rows, schema=tariff_schema)

    # Act
    actual = sum_within_month(
        calculate_tariff_price_per_ga_co_es(tariffs),
        DEFAULT_PERIOD_START_DATETIME,
    )

    # Assert
    assert actual.collect()[0][Colname.charge_time] == datetime(2019, 12, 31, 23)


def test__sum_within_month__sums_quantity_per_month(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        _create_tariff_hour_row(quantity=Decimal("1.111")),
        _create_tariff_hour_row(quantity=Decimal("1.111")),
    ]
    tariffs = spark.createDataFrame(data=rows, schema=tariff_schema)

    # Act
    actual = sum_within_month(
        calculate_tariff_price_per_ga_co_es(tariffs),
        DEFAULT_PERIOD_START_DATETIME,
    )

    # Assert
    assert actual.collect()[0][Colname.total_quantity] == Decimal("2.222")
    assert actual.count() == 1


def test__sum_within_month__sums_charge_price_per_month(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        _create_tariff_hour_row(
            charge_time=datetime(2020, 1, 1, 0), charge_price=Decimal("1.111111")
        ),
        _create_tariff_hour_row(
            charge_time=datetime(2020, 1, 1, 1), charge_price=Decimal("1.111111")
        ),
    ]
    tariffs = spark.createDataFrame(data=rows, schema=tariff_schema)

    # Act
    actual = sum_within_month(
        calculate_tariff_price_per_ga_co_es(tariffs),
        DEFAULT_PERIOD_START_DATETIME,
    )

    # Assert
    assert actual.collect()[0][Colname.charge_price] == Decimal("2.222222")
    assert actual.count() == 1
