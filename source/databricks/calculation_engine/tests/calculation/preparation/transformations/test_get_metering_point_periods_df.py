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

from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import lit, when, col

import pytest
from unittest.mock import patch, Mock

from package import calculation_input
from package.calculation_input.table_reader import TableReader
from package.calculation.preparation.transformations import (
    get_metering_point_periods_df,
)
from package.codelists import (
    InputMeteringPointType,
    InputSettlementMethod,
    MeteringPointType,
    SettlementMethod,
)
from package.constants import Colname

import tests.calculation.preparation.transformations.metering_point_periods_factory as factory
import tests.calculation_input.table_reader.input_metering_point_periods_factory as input_factory
from tests.helpers.data_frame_utils import assert_dataframes_equal

june_1th = datetime(2022, 5, 31, 22, 0)
june_2th = june_1th + timedelta(days=1)
june_3th = june_1th + timedelta(days=2)
june_4th = june_1th + timedelta(days=3)


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


class TestWhenValidInput:
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
    @patch.object(calculation_input, TableReader.__name__)
    def test_returns_df_with_correct_metering_point_types(
        self,
        mock_calculation_input_reader: Mock,
        spark: SparkSession,
        metering_point_type: InputMeteringPointType,
        expected: MeteringPointType,
    ) -> None:
        # Arrange
        row = input_factory.create_row(
            metering_point_type=metering_point_type,
        )
        mock_calculation_input_reader.read_metering_point_periods.return_value = (
            input_factory.create(spark, row)
        )

        # Act
        actual = get_metering_point_periods_df(
            mock_calculation_input_reader,
            input_factory.DEFAULT_FROM_DATE,
            input_factory.DEFAULT_TO_DATE,
            [input_factory.DEFAULT_GRID_AREA],
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
    @patch.object(calculation_input, TableReader.__name__)
    def test_returns_df_with_correct_settlement_methods(
        self,
        mock_calculation_input_reader: Mock,
        spark: SparkSession,
        settlement_method: InputSettlementMethod,
        expected: SettlementMethod,
    ) -> None:
        # Arrange
        row = input_factory.create_row(settlement_method=settlement_method)
        mock_calculation_input_reader.read_metering_point_periods.return_value = (
            input_factory.create(spark, row)
        )

        # Act
        actual = get_metering_point_periods_df(
            mock_calculation_input_reader,
            input_factory.DEFAULT_FROM_DATE,
            input_factory.DEFAULT_TO_DATE,
            [input_factory.DEFAULT_GRID_AREA],
        )

        # Assert
        assert actual.collect()[0][Colname.settlement_method] == expected.value

    @patch.object(calculation_input, TableReader.__name__)
    def test_returns_dataframe_with_expected_columns(
        self,
        mock_calculation_input_reader: Mock,
        spark: SparkSession,
    ) -> None:
        # Arrange
        row = input_factory.create_row(
            metering_point_type=InputMeteringPointType.CONSUMPTION,
            settlement_method=InputSettlementMethod.FLEX,
        )
        mock_calculation_input_reader.read_metering_point_periods.return_value = (
            input_factory.create(spark, row)
        )

        # Act
        actual = get_metering_point_periods_df(
            mock_calculation_input_reader,
            input_factory.DEFAULT_FROM_DATE,
            input_factory.DEFAULT_TO_DATE,
            [input_factory.DEFAULT_GRID_AREA],
        )

        # Assert
        actual_rows = actual.collect()
        assert len(actual_rows) == 1
        actual_row = actual_rows[0]
        assert (
            actual_row[Colname.metering_point_id]
            == input_factory.DEFAULT_METERING_POINT_ID
        )
        assert (
            actual_row[Colname.metering_point_type]
            == MeteringPointType.CONSUMPTION.value
        )
        assert actual_row[Colname.settlement_method] == SettlementMethod.FLEX.value
        assert actual_row[Colname.grid_area] == input_factory.DEFAULT_GRID_AREA
        assert actual_row[Colname.resolution] == input_factory.DEFAULT_RESOLUTION.value
        assert (
            actual_row[Colname.from_grid_area] == input_factory.DEFAULT_FROM_GRID_AREA
        )
        assert actual_row[Colname.to_grid_area] == input_factory.DEFAULT_TO_GRID_AREA
        assert (
            actual_row[Colname.parent_metering_point_id]
            == input_factory.DEFAULT_PARENT_METERING_POINT_ID
        )
        assert (
            actual_row[Colname.energy_supplier_id]
            == input_factory.DEFAULT_ENERGY_SUPPLIER_ID
        )
        assert (
            actual_row[Colname.balance_responsible_id]
            == input_factory.DEFAULT_BALANCE_RESPONSIBLE_ID
        )

    @patch.object(calculation_input, TableReader.__name__)
    def test_returns_expected_df(
        self,
        mock_calculation_input_reader: Mock,
        spark: SparkSession,
    ) -> None:
        # Arrange
        row = input_factory.create_row(
            metering_point_type=InputMeteringPointType.CONSUMPTION,
            settlement_method=InputSettlementMethod.FLEX,
        )
        df = input_factory.create(spark, row)
        mock_calculation_input_reader.read_metering_point_periods.return_value = df
        expected = _map_metering_point_type_and_settlement_method(df)

        # Act
        actual = get_metering_point_periods_df(
            mock_calculation_input_reader,
            input_factory.DEFAULT_FROM_DATE,
            input_factory.DEFAULT_TO_DATE,
            [input_factory.DEFAULT_GRID_AREA],
        )

        # Assert
        assert_dataframes_equal(actual, expected)

    @pytest.mark.parametrize(
        "from_date, to_date, period_start, period_end, expected_from_date, expected_to_date",
        [
            (
                june_1th,
                june_4th,
                june_2th,
                june_3th,
                june_2th,
                june_3th,
            ),  # period is within metering point from/to dates
            (
                june_2th,
                june_4th,
                june_1th,
                june_3th,
                june_2th,
                june_3th,
            ),  # period starts before metering point from date
            (
                june_1th,
                june_3th,
                june_1th,
                june_4th,
                june_1th,
                june_3th,
            ),  # period ends after metering point from/to dates
            (
                june_1th,
                june_3th,
                june_1th,
                june_3th,
                june_1th,
                june_3th,
            ),  # period matches from/to dates
            (
                june_1th,
                None,
                june_1th,
                june_4th,
                june_1th,
                june_4th,
            ),  # period starts at metering point from date and has no end date
        ],
    )
    @patch.object(calculation_input, TableReader.__name__)
    def test_returns_dataframe_with_expect_from_and_to_date(
        self,
        mock_calculation_input_reader: Mock,
        spark: SparkSession,
        from_date: datetime,
        to_date: datetime,
        period_start: datetime,
        period_end: datetime,
        expected_from_date: datetime,
        expected_to_date: datetime,
    ) -> None:
        # Arrange
        row = input_factory.create_row(from_date=from_date, to_date=to_date)
        mock_calculation_input_reader.read_metering_point_periods.return_value = (
            input_factory.create(spark, row)
        )

        # Act
        actual = get_metering_point_periods_df(
            mock_calculation_input_reader,
            period_start,
            period_end,
            [input_factory.DEFAULT_GRID_AREA],
        )

        # Assert
        actual_rows = actual.collect()
        assert len(actual_rows) == 1
        assert actual_rows[0][Colname.from_date] == expected_from_date
        assert actual_rows[0][Colname.to_date] == expected_to_date


class TestWhenThreeGridAreasExchangingWithEachOther:
    @patch.object(calculation_input, TableReader.__name__)
    def test_returns_expected(
        self,
        mock_calculation_input_reader: Mock,
        spark: SparkSession,
    ) -> None:
        # Arrange
        rows = [
            input_factory.create_row(
                grid_area="111", from_grid_area="111", to_grid_area="222"
            ),
            input_factory.create_row(
                grid_area="222", from_grid_area="222", to_grid_area="111"
            ),
            input_factory.create_row(
                grid_area="333", from_grid_area="111", to_grid_area="222"
            ),
        ]

        mock_calculation_input_reader.read_metering_point_periods.return_value = (
            input_factory.create(spark, rows)
        )

        # Act
        actual = get_metering_point_periods_df(
            mock_calculation_input_reader,
            input_factory.DEFAULT_FROM_DATE,
            input_factory.DEFAULT_TO_DATE,
            ["111", "222"],
        )

        # Assert
        actual_rows = sorted(actual.collect(), key=lambda x: x[Colname.grid_area])
        assert len(actual_rows) == 3
        assert actual_rows[0][Colname.grid_area] == "111"
        assert actual_rows[0][Colname.from_grid_area] == "111"
        assert actual_rows[0][Colname.to_grid_area] == "222"
        assert actual_rows[1][Colname.grid_area] == "222"
        assert actual_rows[1][Colname.from_grid_area] == "222"
        assert actual_rows[1][Colname.to_grid_area] == "111"
        assert actual_rows[2][Colname.grid_area] == "333"
        assert actual_rows[2][Colname.from_grid_area] == "111"
        assert actual_rows[2][Colname.to_grid_area] == "222"


class TestWhenSchemaMismatch:
    @patch.object(calculation_input, TableReader.__name__)
    def test_raises_assertion_error(
        self, mock_calculation_input_reader: Mock, spark: SparkSession
    ) -> None:
        # Arrange
        row = input_factory.create_row()
        df = input_factory.create(spark, row)
        df = df.withColumn("test", lit("test"))

        mock_calculation_input_reader.read_metering_point_periods.return_value = df

        # Act & Assert
        with pytest.raises(AssertionError) as exc_info:
            get_metering_point_periods_df(
                mock_calculation_input_reader,
                input_factory.DEFAULT_FROM_DATE,
                input_factory.DEFAULT_TO_DATE,
                [input_factory.DEFAULT_GRID_AREA],
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
    @patch.object(calculation_input, TableReader.__name__)
    def test_returns_metering_point_if_it_associates_to_relevant_grid_area(
        self,
        mock_calculation_input_reader: Mock,
        spark: SparkSession,
        grid_area: str,
        from_grid_area: str,
        to_grid_area: str,
        calculation_grid_area: str,
        expected: bool,
    ) -> None:
        # Arrange
        row = input_factory.create_row(
            metering_point_type=InputMeteringPointType.EXCHANGE,
            grid_area=grid_area,
            from_grid_area=from_grid_area,
            to_grid_area=to_grid_area,
        )
        mock_calculation_input_reader.read_metering_point_periods.return_value = (
            input_factory.create(spark, row)
        )

        # Act
        actual = get_metering_point_periods_df(
            mock_calculation_input_reader,
            input_factory.DEFAULT_FROM_DATE,
            input_factory.DEFAULT_TO_DATE,
            [calculation_grid_area],
        )

        # Assert
        assert len(actual.collect()) == expected
