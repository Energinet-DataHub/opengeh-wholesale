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
from unittest import mock
import pytest
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import lit

from package.codelists import (
    InputMeteringPointType,
    InputSettlementMethod,
    MeteringPointType,
    SettlementMethod,
)
from package.calculation_input.calculation_input_reader import CalculationInputReader
from package.calculation_input.schemas import (
    metering_point_period_schema,
    charge_price_points_schema,
    time_series_point_schema,
    charge_link_periods_schema,
    charge_master_data_periods_schema,
)
from package.constants import Colname
from pyspark.sql.types import StructType


def _create_metering_point_period_row(
    metering_point_type: InputMeteringPointType = InputMeteringPointType.CONSUMPTION,
    settlement_method: InputSettlementMethod = InputSettlementMethod.FLEX,
) -> dict:
    return {
        Colname.metering_point_id: "foo",
        Colname.metering_point_type: metering_point_type.value,
        Colname.calculation_type: "foo",
        Colname.settlement_method: settlement_method.value,
        Colname.grid_area: "foo",
        Colname.resolution: "foo",
        Colname.from_grid_area: "foo",
        Colname.to_grid_area: "foo",
        Colname.parent_metering_point_id: "foo",
        Colname.energy_supplier_id: "foo",
        Colname.balance_responsible_id: "foo",
        Colname.from_date: datetime(2022, 6, 8, 22, 0, 0),
        Colname.to_date: datetime(2022, 6, 8, 22, 0, 0),
    }


def _create_change_price_point_row() -> dict:
    return {
        Colname.charge_id: "foo",
        Colname.charge_type: "foo",
        Colname.charge_owner: "foo",
        Colname.charge_price: Decimal("1.123456"),
        Colname.observation_time: datetime(2022, 6, 8, 22, 0, 0),
    }


def _create_time_series_point_row() -> dict:
    return {
        Colname.metering_point_id: "foo",
        Colname.quantity: Decimal("1.123456"),
        Colname.quality: "foo",
        Colname.observation_time: datetime(2022, 6, 8, 22, 0, 0),
    }


def _create_charge_link_period_row() -> dict:
    return {
        Colname.charge_id: "foo",
        Colname.charge_type: "foo",
        Colname.charge_owner: "foo",
        Colname.metering_point_id: "foo",
        Colname.quantity: 1,
        Colname.from_date: datetime(2022, 6, 8, 22, 0, 0),
        Colname.to_date: datetime(2022, 6, 8, 22, 0, 0),
    }


def _create_charge_master_period_row() -> dict:
    return {
        Colname.charge_id: "foo",
        Colname.charge_type: "foo",
        Colname.charge_owner: "foo",
        Colname.resolution: "foo",
        Colname.charge_tax: False,
        Colname.from_date: datetime(2022, 6, 8, 22, 0, 0),
        Colname.to_date: datetime(2022, 6, 8, 22, 0, 0),
    }


@pytest.mark.parametrize(
    "metering_point_type,expected",
    [
        [InputMeteringPointType.CONSUMPTION, MeteringPointType.CONSUMPTION],
        [InputMeteringPointType.PRODUCTION, MeteringPointType.PRODUCTION],
        [InputMeteringPointType.EXCHANGE, MeteringPointType.EXCHANGE],
        [InputMeteringPointType.VE_PRODUCTION, MeteringPointType.VE_PRODUCTION],
        [InputMeteringPointType.NET_PRODUCTION, MeteringPointType.NET_PRODUCTION],
        [InputMeteringPointType.SUPPLY_TO_GRID, MeteringPointType.SUPPLY_TO_GRID],
        [
            InputMeteringPointType.CONSUMPTION_FROM_GRID,
            MeteringPointType.CONSUMPTION_FROM_GRID,
        ],
        [
            InputMeteringPointType.WHOLESALE_SERVICES_INFORMATION,
            MeteringPointType.WHOLESALE_SERVICES_INFORMATION,
        ],
        [InputMeteringPointType.OWN_PRODUCTION, MeteringPointType.OWN_PRODUCTION],
        [InputMeteringPointType.NET_FROM_GRID, MeteringPointType.NET_FROM_GRID],
        [InputMeteringPointType.NET_TO_GRID, MeteringPointType.NET_TO_GRID],
        [InputMeteringPointType.TOTAL_CONSUMPTION, MeteringPointType.TOTAL_CONSUMPTION],
        [
            InputMeteringPointType.ELECTRICAL_HEATING,
            MeteringPointType.ELECTRICAL_HEATING,
        ],
        [InputMeteringPointType.NET_CONSUMPTION, MeteringPointType.NET_CONSUMPTION],
        [InputMeteringPointType.EFFECT_SETTLEMENT, MeteringPointType.EFFECT_SETTLEMENT],
    ],
)
def test___read_metering_point_periods__returns_df_with_correct_metering_point_types(
    spark: SparkSession,
    metering_point_type: InputMeteringPointType,
    expected: MeteringPointType,
) -> None:
    # Arrange
    row = _create_metering_point_period_row(metering_point_type=metering_point_type)
    df = spark.createDataFrame(data=[row], schema=metering_point_period_schema)
    sut = CalculationInputReader(spark)

    # Act
    with mock.patch.object(sut, "_read_table", return_value=df):
        actual = sut.read_metering_point_periods()

    # Assert
    assert actual.collect()[0][Colname.metering_point_type] == expected.value


@pytest.mark.parametrize(
    "settlement_method,expected",
    [
        [InputSettlementMethod.FLEX, SettlementMethod.FLEX],
        [InputSettlementMethod.NON_PROFILED, SettlementMethod.NON_PROFILED],
    ],
)
def test___read_metering_point_periods__returns_df_with_correct_settlemet_methods(
    spark: SparkSession,
    settlement_method: InputSettlementMethod,
    expected: SettlementMethod,
) -> None:
    row = _create_metering_point_period_row(settlement_method=settlement_method)
    df = spark.createDataFrame(data=[row], schema=metering_point_period_schema)
    sut = CalculationInputReader(spark)

    # Act
    with mock.patch.object(sut, "_read_table", return_value=df):
        actual = sut.read_metering_point_periods()

    # Assert
    assert actual.collect()[0][Colname.settlement_method] == expected.value


@pytest.mark.parametrize(
    "settlement_method,expected",
    [
        [InputSettlementMethod.FLEX, SettlementMethod.FLEX],
        [InputSettlementMethod.NON_PROFILED, SettlementMethod.NON_PROFILED],
    ],
)
def test___read_metering_point_periods__returns_df_with_correct_settlement_methods(
    spark: SparkSession,
    settlement_method: InputSettlementMethod,
    expected: SettlementMethod,
) -> None:
    row = _create_metering_point_period_row(settlement_method=settlement_method)
    df = spark.createDataFrame(data=[row], schema=metering_point_period_schema)
    sut = CalculationInputReader(spark)

    # Act
    with mock.patch.object(sut, "_read_table", return_value=df):
        actual = sut.read_metering_point_periods()

    # Assert
    assert actual.collect()[0][Colname.settlement_method] == expected.value


@pytest.mark.parametrize(
    "expected_schema, method_name, create_row",
    [
        (
            metering_point_period_schema,
            CalculationInputReader.read_metering_point_periods,
            _create_metering_point_period_row,
        ),
        (
            time_series_point_schema,
            CalculationInputReader.read_time_series_points,
            _create_time_series_point_row,
        ),
        (
            charge_master_data_periods_schema,
            CalculationInputReader.read_charge_master_data_periods,
            _create_charge_master_period_row,
        ),
        (
            charge_link_periods_schema,
            CalculationInputReader.read_charge_links_periods,
            _create_charge_link_period_row,
        ),
        (
            charge_price_points_schema,
            CalculationInputReader.read_charge_price_points,
            _create_change_price_point_row,
        ),
    ],
)
def test__read_data__returns_df(
    spark: SparkSession, expected_schema: StructType, method_name: str, create_row: any
) -> None:
    # Arrange
    row = create_row()
    reader = CalculationInputReader(spark)
    df = spark.createDataFrame(data=[row], schema=expected_schema)
    sut = getattr(reader, method_name.__name__)

    # Act
    with mock.patch.object(reader, "_read_table", return_value=df):
        actual = sut()

    # Assert
    assert isinstance(actual, DataFrame)


@pytest.mark.parametrize(
    "expected_schema, method_name, create_row",
    [
        (
            metering_point_period_schema,
            CalculationInputReader.read_metering_point_periods,
            _create_metering_point_period_row,
        ),
        (
            time_series_point_schema,
            CalculationInputReader.read_time_series_points,
            _create_time_series_point_row,
        ),
        (
            charge_master_data_periods_schema,
            CalculationInputReader.read_charge_master_data_periods,
            _create_charge_master_period_row,
        ),
        (
            charge_link_periods_schema,
            CalculationInputReader.read_charge_links_periods,
            _create_charge_link_period_row,
        ),
        (
            charge_price_points_schema,
            CalculationInputReader.read_charge_price_points,
            _create_change_price_point_row,
        ),
    ],
)
def test__read_data__raises_value_error_when_schema_mismatch(
    spark: SparkSession, expected_schema: StructType, method_name: str, create_row: any
) -> None:
    # Arrange
    row = create_row()
    sut = CalculationInputReader(spark)
    df = spark.createDataFrame(data=[row], schema=expected_schema)
    df = df.withColumn("test", lit("test"))
    method = getattr(sut, method_name.__name__)
    is_exception_thrown = False

    # Act
    with mock.patch.object(sut, "_read_table", return_value=df):
        try:
            method()
            print("This test fails because the schemas are identical!")
        except Exception:
            is_exception_thrown = True

    # Assert
    if is_exception_thrown:
        assert True
    else:
        assert False


@pytest.mark.parametrize(
    "expected_schema, method_name, create_row",
    [
        (
            metering_point_period_schema,
            CalculationInputReader.read_metering_point_periods,
            _create_metering_point_period_row,
        ),
        (
            time_series_point_schema,
            CalculationInputReader.read_time_series_points,
            _create_time_series_point_row,
        ),
        (
            charge_master_data_periods_schema,
            CalculationInputReader.read_charge_master_data_periods,
            _create_charge_master_period_row,
        ),
        (
            charge_link_periods_schema,
            CalculationInputReader.read_charge_links_periods,
            _create_charge_link_period_row,
        ),
        (
            charge_price_points_schema,
            CalculationInputReader.read_charge_price_points,
            _create_change_price_point_row,
        ),
    ],
)
def test__read_data__throws_exception_when_schema_mismatch2(
    spark: SparkSession,
    expected_schema: StructType,
    method_name: str,
    create_row: any,
) -> None:
    # Arrange
    row = create_row()
    reader = CalculationInputReader(spark)
    df = spark.createDataFrame(data=[row], schema=expected_schema)
    df = df.withColumn("test", lit("test"))
    sut = getattr(reader, str(method_name.__name__))

    # Act & Assert
    with mock.patch.object(reader, "_read_table", return_value=df):
        with pytest.raises(Exception):
            sut()
