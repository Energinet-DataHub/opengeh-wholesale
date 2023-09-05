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
from pyspark.sql import SparkSession, DataFrame
import pytest
from typing import Any, List, Union

from package.codelists import (
    ChargeQuality,
    ChargeResolution,
    ChargeType,
    ChargeUnit,
    MeteringPointType,
    SettlementMethod
)
from package.calculation.wholesale.tariff_calculators import (
    tariff_schema,
    calculate_tariff_price_per_ga_co_es,
)
from package.constants import Colname


DEFAULT_GRID_AREA = "543"
DEFAULT_CHARGE_ID = "4000"
DEFAULT_CHARGE_OWNER = "001"
DEFAULT_CHARGE_TAX = True
DEFAULT_CHARGE_TIME_HOUR_0 = datetime(2020, 1, 1, 0)
DEFAULT_CHARGE_PRICE = Decimal("2.000005")
DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"
DEFAULT_METERING_POINT_ID = "123456789012345678901234567"
DEFAULT_METERING_POINT_TYPE = MeteringPointType.CONSUMPTION
DEFAULT_SETTLEMENT_METHOD = SettlementMethod.FLEX
DEFAULT_QUANTITY = Decimal("1.005")


def _create_tariff_hour_row(
    charge_key: Union[str, None] = None,
    charge_id: str = DEFAULT_CHARGE_ID,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_time: datetime = DEFAULT_CHARGE_TIME_HOUR_0,
    charge_price: Decimal = DEFAULT_CHARGE_PRICE,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    metering_point_type: MeteringPointType = DEFAULT_METERING_POINT_TYPE,
    settlement_method: SettlementMethod = DEFAULT_SETTLEMENT_METHOD,
    grid_area: str = DEFAULT_GRID_AREA,
    quantity: Decimal = DEFAULT_QUANTITY,
) -> dict:
    row = {
        Colname.charge_key: charge_key or f"{charge_id}-{ChargeType.TARIFF.value}-{charge_owner}",
        Colname.charge_id: charge_id,
        Colname.charge_type: ChargeType.TARIFF.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_tax: DEFAULT_CHARGE_TAX,
        Colname.charge_resolution: ChargeResolution.HOUR.value,
        Colname.charge_time: charge_time,
        Colname.charge_price: charge_price,
        Colname.metering_point_id: metering_point_id,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.metering_point_type: metering_point_type.value,
        Colname.settlement_method: settlement_method.value,
        Colname.grid_area: grid_area,
        Colname.quantity: quantity,
    }

    return row


def _create_df(spark: SparkSession, row: List[dict]) -> DataFrame:
    return spark.createDataFrame(data=row, schema=tariff_schema)


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
    assert Colname.charge_id in actual.columns
    assert Colname.charge_type in actual.columns
    assert Colname.charge_owner in actual.columns
    assert Colname.charge_tax in actual.columns
    assert Colname.charge_resolution in actual.columns
    assert Colname.charge_price in actual.columns
    assert Colname.total_quantity in actual.columns
    assert Colname.charge_count in actual.columns
    assert Colname.total_amount in actual.columns
    assert Colname.unit in actual.columns
    assert Colname.quality in actual.columns


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
    assert actual_row[Colname.charge_id] == DEFAULT_CHARGE_ID
    assert actual_row[Colname.charge_type] == ChargeType.TARIFF.value
    assert actual_row[Colname.charge_owner] == DEFAULT_CHARGE_OWNER
    assert actual_row[Colname.charge_tax] == DEFAULT_CHARGE_TAX
    assert actual_row[Colname.charge_resolution] == ChargeResolution.HOUR.value
    assert actual_row[Colname.charge_price] == DEFAULT_CHARGE_PRICE
    assert actual_row[Colname.total_quantity] == 3 * DEFAULT_QUANTITY
    assert actual_row[Colname.charge_count] == 3
    assert actual_row[Colname.total_amount] == Decimal("6.030015")  # 3 * DEFAULT_CHARGE_PRICE * DEFAULT_QUANTITY rounded to 6 decimals
    assert actual_row[Colname.unit] == ChargeUnit.KWH.value
    assert actual_row[Colname.quality] == ChargeQuality.CALCULATED.value


@pytest.mark.parametrize(
    "column_name, value, other_value",
    [
        ("grid_area", "1", "2"),
        ("energy_supplier_id", "1", "2"),
        ("metering_point_type", MeteringPointType.CONSUMPTION, MeteringPointType.PRODUCTION),
        ("settlement_method", SettlementMethod.FLEX, SettlementMethod.NON_PROFILED),
        ("charge_key", "1", "2"),
        ("charge_id", "1", "2"),
        ("charge_owner", "1", "2"),
        ("charge_time", DEFAULT_CHARGE_TIME_HOUR_0, DEFAULT_CHARGE_TIME_HOUR_0 + timedelta(hours=1)),
    ]
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


@pytest.mark.parametrize(
    "column_name, expected_precision",
    [
        (Colname.total_amount, 6),
        (Colname.quantity, 3),
        (Colname.charge_price, 6),
    ]
)
def test__calculate_tariff_price_per_ga_co_es__returns_df_with_expected_precisions(
    spark: SparkSession, column_name: str, expected_precision: int
) -> None:
    # Arrange
    rows = [_create_tariff_hour_row()]
    tariffs = spark.createDataFrame(data=rows, schema=tariff_schema)

    # Act
    actual = calculate_tariff_price_per_ga_co_es(tariffs)

    # Assert
    assert actual.schema[column_name].precision == expected_precision
