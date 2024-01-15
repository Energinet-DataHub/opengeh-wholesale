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
from unittest import mock
import pytest
from pyspark.sql import SparkSession, DataFrame
import pyspark.sql.functions as f

from package.codelists import (
    InputMeteringPointType,
    InputSettlementMethod,
    MeteringPointType,
    SettlementMethod,
)
from package.calculation_input.table_reader import TableReader
from package.calculation_input.schemas import metering_point_period_schema
from package.constants import Colname
from tests.helpers.delta_table_utils import write_dataframe_to_table
from tests.helpers.data_frame_utils import assert_dataframes_equal

DEFAULT_FROM_DATE = datetime(2022, 6, 8, 22, 0, 0)
DEFAULT_TO_DATE = datetime(2022, 6, 9, 22, 0, 0)
DEFAULT_GRID_AREA = "805"


def _create_metering_point_period_row(
    metering_point_type: InputMeteringPointType = InputMeteringPointType.CONSUMPTION,
    settlement_method: InputSettlementMethod = InputSettlementMethod.FLEX,
    grid_area: str = DEFAULT_GRID_AREA,
    from_grid_area: str = None,
    to_grid_area: str = None,
) -> dict:
    return {
        Colname.metering_point_id: "foo",
        Colname.metering_point_type: metering_point_type.value,
        Colname.calculation_type: "foo",
        Colname.settlement_method: settlement_method.value,
        Colname.grid_area: grid_area,
        Colname.resolution: "foo",
        Colname.from_grid_area: from_grid_area,
        Colname.to_grid_area: to_grid_area,
        Colname.parent_metering_point_id: "foo",
        Colname.energy_supplier_id: "foo",
        Colname.balance_responsible_id: "foo",
        Colname.from_date: DEFAULT_FROM_DATE,
        Colname.to_date: DEFAULT_TO_DATE,
    }


def _map_metering_point_type_and_settlement_method(df: DataFrame) -> DataFrame:
    """
    Maps metering point type and settlement method to the correct values
    Currently only supports consumption and flex
    """
    return df.withColumn(
        Colname.metering_point_type,
        f.when(
            f.col(Colname.metering_point_type)
            == InputMeteringPointType.CONSUMPTION.value,
            MeteringPointType.CONSUMPTION.value,
        ).otherwise(f.col(Colname.metering_point_type)),
    ).withColumn(
        Colname.settlement_method,
        f.when(
            f.col(Colname.settlement_method) == InputSettlementMethod.FLEX.value,
            SettlementMethod.FLEX.value,
        ).otherwise(f.col(Colname.settlement_method)),
    )


class TestWhenValidInput:
    def test_returns_expected_df(
        self,
        spark: SparkSession,
        tmp_path: pathlib.Path,
        calculation_input_folder: str,
    ) -> None:
        # Arrange
        calculation_input_path = f"{str(tmp_path)}/{calculation_input_folder}"
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
        actual = reader.read_metering_point_periods(
            DEFAULT_FROM_DATE,
            DEFAULT_TO_DATE,
            [DEFAULT_GRID_AREA],
        )

        # Assert
        assert_dataframes_equal(actual, expected)

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
            [
                InputMeteringPointType.TOTAL_CONSUMPTION,
                MeteringPointType.TOTAL_CONSUMPTION,
            ],
            [
                InputMeteringPointType.ELECTRICAL_HEATING,
                MeteringPointType.ELECTRICAL_HEATING,
            ],
            [InputMeteringPointType.NET_CONSUMPTION, MeteringPointType.NET_CONSUMPTION],
            [
                InputMeteringPointType.EFFECT_SETTLEMENT,
                MeteringPointType.EFFECT_SETTLEMENT,
            ],
        ],
    )
    def test_returns_df_with_correct_metering_point_types(
        self,
        spark: SparkSession,
        metering_point_type: InputMeteringPointType,
        expected: MeteringPointType,
    ) -> None:
        # Arrange
        row = _create_metering_point_period_row(metering_point_type=metering_point_type)
        df = spark.createDataFrame(data=[row], schema=metering_point_period_schema)
        sut = TableReader(mock.Mock(), "dummy_calculation_input_path")

        # Act
        with mock.patch.object(
            sut._spark.read.format("delta"), "load", return_value=df
        ):
            actual = sut.read_metering_point_periods(
                DEFAULT_FROM_DATE,
                DEFAULT_TO_DATE,
                [DEFAULT_GRID_AREA],
            )

        # Assert
        assert actual.collect()[0][Colname.metering_point_type] == expected.value

    @pytest.mark.parametrize(
        "settlement_method,expected",
        [
            [InputSettlementMethod.FLEX, SettlementMethod.FLEX],
            [InputSettlementMethod.NON_PROFILED, SettlementMethod.NON_PROFILED],
        ],
    )
    def test_returns_df_with_correct_settlement_methods(
        self,
        spark: SparkSession,
        settlement_method: InputSettlementMethod,
        expected: SettlementMethod,
    ) -> None:
        row = _create_metering_point_period_row(settlement_method=settlement_method)
        df = spark.createDataFrame(data=[row], schema=metering_point_period_schema)
        sut = TableReader(mock.Mock(), "dummy_calculation_input_path")

        # Act
        with mock.patch.object(
            sut._spark.read.format("delta"), "load", return_value=df
        ):
            actual = sut.read_metering_point_periods(
                DEFAULT_FROM_DATE,
                DEFAULT_TO_DATE,
                [DEFAULT_GRID_AREA],
            )

        # Assert
        assert actual.collect()[0][Colname.settlement_method] == expected.value


class TestWhenSchemaMismatch:
    def test_raises_assertion_error(self, spark: SparkSession) -> None:
        # Arrange
        row = _create_metering_point_period_row()
        reader = TableReader(mock.Mock(), "dummy_calculation_input_path")
        df = spark.createDataFrame(data=[row], schema=metering_point_period_schema)
        df = df.withColumn("test", f.lit("test"))

        # Act & Assert
        with mock.patch.object(
            reader._spark.read.format("delta"), "load", return_value=df
        ):
            with pytest.raises(AssertionError) as exc_info:
                reader.read_metering_point_periods(
                    DEFAULT_FROM_DATE,
                    DEFAULT_TO_DATE,
                    [DEFAULT_GRID_AREA],
                )

            assert "Schema mismatch" in str(exc_info.value)


class TestWhenExchangeMeteringPoint:
    @pytest.mark.parametrize(
        "grid_area, from_grid_area, to_grid_area, calculation_grid_area, expected",
        [
            ("111", "222", "333", "111", 1),
            ("111", "222", "333", "222", 1),
            ("111", "222", "333", "333", 1),
            ("111", "222", "333", "444", 0),
            ("111", "111", "333", "111", 1),
            ("111", "222", "111", "111", 1),
        ],
    )
    def test_returns_metering_point_if_it_associates_to_relevant_grid_area(
        self,
        spark: SparkSession,
        grid_area: str,
        from_grid_area: str,
        to_grid_area: str,
        calculation_grid_area: str,
        expected: bool,
    ) -> None:
        # Arrange
        row = _create_metering_point_period_row(
            metering_point_type=InputMeteringPointType.EXCHANGE,
            grid_area=grid_area,
            from_grid_area=from_grid_area,
            to_grid_area=to_grid_area,
        )
        df = spark.createDataFrame(data=[row], schema=metering_point_period_schema)

        sut = TableReader(mock.Mock(), "dummy_calculation_input_path")

        # Act
        with mock.patch.object(
            sut._spark.read.format("delta"), "load", return_value=df
        ):
            actual = sut.read_metering_point_periods(
                DEFAULT_FROM_DATE,
                DEFAULT_TO_DATE,
                [calculation_grid_area],
            )

        # Assert
        assert len(actual.collect()) == expected
