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
import pathlib
from datetime import datetime
from decimal import Decimal
from unittest import mock
import pytest
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import StructType
from pyspark.sql.functions import col, when, lit

from package.codelists import (
    InputMeteringPointType,
    InputSettlementMethod,
    MeteringPointType,
    SettlementMethod,
)
from package.calculation_input.table_reader import TableReader
from package.calculation_input.schemas import (
    metering_point_period_schema,
    charge_price_points_schema,
    time_series_point_schema,
    charge_link_periods_schema,
    charge_master_data_periods_schema,
)
from package.constants import Colname
from tests.helpers.delta_table_utils import write_dataframe_to_table
from tests.helpers.data_frame_utils import assert_dataframes_equal


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
        Colname.charge_code: "foo",
        Colname.charge_type: "foo",
        Colname.charge_owner: "foo",
        Colname.charge_price: Decimal("1.123456"),
        Colname.charge_time: datetime(2022, 6, 8, 22, 0, 0),
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
        Colname.charge_code: "foo",
        Colname.charge_type: "foo",
        Colname.charge_owner: "foo",
        Colname.metering_point_id: "foo",
        Colname.quantity: 1,
        Colname.from_date: datetime(2022, 6, 8, 22, 0, 0),
        Colname.to_date: datetime(2022, 6, 8, 22, 0, 0),
    }


def _create_charge_master_period_row() -> dict:
    return {
        Colname.charge_code: "foo",
        Colname.charge_type: "foo",
        Colname.charge_owner: "foo",
        Colname.resolution: "foo",
        Colname.charge_tax: False,
        Colname.from_date: datetime(2022, 6, 8, 22, 0, 0),
        Colname.to_date: datetime(2022, 6, 8, 22, 0, 0),
    }


def _map_metering_point_type_and_settlement_method(df: DataFrame) -> DataFrame:
    """
    Maps metering point type and settlement method to the correct values
    Currently only supports consumption and flex
    """
    return df.withColumn(
        Colname.metering_point_type,
        when(
            col(Colname.metering_point_type)
            == InputMeteringPointType.CONSUMPTION.value,
            MeteringPointType.CONSUMPTION.value,
        ).otherwise(col(Colname.metering_point_type)),
    ).withColumn(
        Colname.settlement_method,
        when(
            col(Colname.settlement_method) == InputSettlementMethod.FLEX.value,
            SettlementMethod.FLEX.value,
        ).otherwise(col(Colname.settlement_method)),
    )


def _add_charge_key(df: DataFrame) -> DataFrame:
    return df.withColumn(Colname.charge_key, lit("foo-foo-foo"))


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
    sut = TableReader(spark, "dummy_calculation_input_path")

    # Act
    with mock.patch.object(sut, TableReader._read_table.__name__, return_value=df):
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
def test___read_metering_point_periods__returns_df_with_correct_settlement_methods(
    spark: SparkSession,
    settlement_method: InputSettlementMethod,
    expected: SettlementMethod,
) -> None:
    row = _create_metering_point_period_row(settlement_method=settlement_method)
    df = spark.createDataFrame(data=[row], schema=metering_point_period_schema)
    sut = TableReader(spark, "dummy_calculation_input_path")

    # Act
    with mock.patch.object(sut, TableReader._read_table.__name__, return_value=df):
        actual = sut.read_metering_point_periods()

    # Assert
    assert actual.collect()[0][Colname.settlement_method] == expected.value


@pytest.mark.parametrize(
    "expected_schema, method_name, create_row",
    [
        (
            metering_point_period_schema,
            TableReader.read_metering_point_periods,
            _create_metering_point_period_row,
        ),
        (
            time_series_point_schema,
            TableReader.read_time_series_points,
            _create_time_series_point_row,
        ),
        (
            charge_master_data_periods_schema,
            TableReader.read_charge_master_data_periods,
            _create_charge_master_period_row,
        ),
        (
            charge_link_periods_schema,
            TableReader.read_charge_links_periods,
            _create_charge_link_period_row,
        ),
        (
            charge_price_points_schema,
            TableReader.read_charge_price_points,
            _create_change_price_point_row,
        ),
    ],
)
def test__read_data__when_schema_mismatch__raises_assertion_error(
    spark: SparkSession, expected_schema: StructType, method_name: str, create_row: any
) -> None:
    # Arrange
    row = create_row()
    reader = TableReader(spark, "dummy_calculation_input_path")
    df = spark.createDataFrame(data=[row], schema=expected_schema)
    df = df.withColumn("test", lit("test"))
    sut = getattr(reader, str(method_name.__name__))

    # Act & Assert
    with mock.patch.object(reader, TableReader._read_table.__name__, return_value=df):
        with pytest.raises(AssertionError):
            sut()


def test__read_metering_point_periods__returns_expected_df(
    spark: SparkSession,
    tmp_path: pathlib.Path,
) -> None:
    # Arrange
    calculation_input_path = f"{str(tmp_path)}/calculation_input"
    table_location = f"{calculation_input_path}/metering_point_periods"
    row = _create_metering_point_period_row()
    df = spark.createDataFrame(data=[row], schema=metering_point_period_schema)
    write_dataframe_to_table(
        spark,
        df,
        "test_database",
        "metering_point_periods",
        table_location,
        metering_point_period_schema,
    )
    expected = _map_metering_point_type_and_settlement_method(df)
    reader = TableReader(spark, calculation_input_path)

    # Act
    actual = reader.read_metering_point_periods()

    # Assert
    assert_dataframes_equal(actual, expected)


def test__read_time_series_points__returns_expected_df(
    spark: SparkSession,
    tmp_path: pathlib.Path,
) -> None:
    # Arrange
    calculation_input_path = f"{str(tmp_path)}/calculation_input"
    table_location = f"{calculation_input_path}/time_series_points"
    row = _create_time_series_point_row()
    df = spark.createDataFrame(data=[row], schema=time_series_point_schema)
    write_dataframe_to_table(
        spark,
        df,
        "test_database",
        "time_series_points",
        table_location,
        time_series_point_schema,
    )
    expected = df
    reader = TableReader(spark, calculation_input_path)

    # Act
    actual = reader.read_time_series_points()

    # Assert
    assert_dataframes_equal(actual, expected)


def test__read_charge_price_points__returns_expected_df(
    spark: SparkSession,
    tmp_path: pathlib.Path,
) -> None:
    # Arrange
    calculation_input_path = f"{str(tmp_path)}/calculation_input"
    table_location = f"{calculation_input_path}/charge_price_points"
    row = _create_change_price_point_row()
    df = spark.createDataFrame(data=[row], schema=charge_price_points_schema)
    write_dataframe_to_table(
        spark,
        df,
        "test_database",
        "charge_price_points",
        table_location,
        charge_price_points_schema,
    )
    expected = _add_charge_key(df)
    reader = TableReader(spark, calculation_input_path)

    # Act
    actual = reader.read_charge_price_points()

    # Assert
    assert_dataframes_equal(actual, expected)


def test__read_charge_master_data_periods__returns_expected_df(
    spark: SparkSession,
    tmp_path: pathlib.Path,
) -> None:
    # Arrange
    calculation_input_path = f"{str(tmp_path)}/calculation_input"
    table_location = f"{calculation_input_path}/charge_masterdata_periods"
    row = _create_charge_master_period_row()
    df = spark.createDataFrame(data=[row], schema=charge_master_data_periods_schema)
    write_dataframe_to_table(
        spark,
        df,
        "test_database",
        "charge_master_data_periods",
        table_location,
        charge_master_data_periods_schema,
    )
    expected = _add_charge_key(df)
    reader = TableReader(spark, calculation_input_path)

    # Act
    actual = reader.read_charge_master_data_periods()

    # Assert
    assert_dataframes_equal(actual, expected)


def test__read_charge_links_periods__returns_expected_df(
    spark: SparkSession,
    tmp_path: pathlib.Path,
) -> None:
    # Arrange
    calculation_input_path = f"{str(tmp_path)}/calculation_input"
    table_location = f"{calculation_input_path}/charge_link_periods"
    row = _create_charge_link_period_row()
    df = spark.createDataFrame(data=[row], schema=charge_link_periods_schema)
    write_dataframe_to_table(
        spark,
        df,
        "test_database",
        "charge_link_periods",
        table_location,
        charge_link_periods_schema,
    )
    expected = _add_charge_key(df)
    reader = TableReader(spark, calculation_input_path)

    # Act
    actual = reader.read_charge_links_periods()

    # Assert
    assert_dataframes_equal(actual, expected)
