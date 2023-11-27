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

from pyspark.sql import SparkSession

import pytest
from unittest.mock import patch, Mock

from package import calculation_input
from package.calculation_input.table_reader import TableReader
from package.calculation.preparation.transformations import (
    get_metering_point_periods_df,
)
from package.codelists import MeteringPointType
from package.constants import Colname
import tests.calculation.preparation.transformations.metering_point_periods_factory as factory


june_1th = datetime(2022, 5, 31, 22, 0)
june_2th = june_1th + timedelta(days=1)
june_3th = june_1th + timedelta(days=2)
june_4th = june_1th + timedelta(days=3)


class TestWhenValidInput:
    @pytest.mark.parametrize("metering_point_type", list(MeteringPointType))
    @patch.object(calculation_input, TableReader.__name__)
    def test_returns_metering_point_period_for_(
        self,
        mock_calculation_input_reader: Mock,
        spark: SparkSession,
        metering_point_type: MeteringPointType,
    ) -> None:
        # Arrange
        row = factory.create_row(
            metering_point_type=metering_point_type,
        )
        mock_calculation_input_reader.read_metering_point_periods.return_value = (
            factory.create(spark, row)
        )

        # Act
        actual = get_metering_point_periods_df(
            mock_calculation_input_reader,
            factory.DEFAULT_FROM_DATE,
            factory.DEFAULT_TO_DATE,
            [factory.DEFAULT_GRID_AREA],
        )

        # Assert
        actual_rows = actual.collect()
        assert len(actual_rows) == 1
        assert actual_rows[0][Colname.metering_point_type] == metering_point_type.value

    @patch.object(calculation_input, TableReader.__name__)
    def test_returns_dataframe_with_expected_columns(
        self,
        mock_calculation_input_reader: Mock,
        spark: SparkSession,
    ) -> None:
        # Arrange
        row = factory.create_row()
        mock_calculation_input_reader.read_metering_point_periods.return_value = (
            factory.create(spark, row)
        )

        # Act
        actual = get_metering_point_periods_df(
            mock_calculation_input_reader,
            factory.DEFAULT_FROM_DATE,
            factory.DEFAULT_TO_DATE,
            [factory.DEFAULT_GRID_AREA],
        )

        # Assert
        actual_rows = actual.collect()
        assert len(actual_rows) == 1
        actual_row = actual_rows[0]
        assert (
            actual_row[Colname.metering_point_id] == factory.DEFAULT_METERING_POINT_ID
        )
        assert (
            actual_row[Colname.metering_point_type]
            == factory.DEFAULT_METERING_POINT_TYPE.value
        )
        assert (
            actual_row[Colname.settlement_method]
            == factory.DEFAULT_SETTLEMENT_METHOD.value
        )
        assert actual_row[Colname.grid_area] == factory.DEFAULT_GRID_AREA
        assert actual_row[Colname.resolution] == factory.DEFAULT_RESOLUTION.value
        assert actual_row[Colname.from_grid_area] == factory.DEFAULT_FROM_GRID_AREA
        assert actual_row[Colname.to_grid_area] == factory.DEFAULT_TO_GRID_AREA
        assert (
            actual_row[Colname.parent_metering_point_id]
            == factory.DEFAULT_PARENT_METERING_POINT_ID
        )
        assert (
            actual_row[Colname.energy_supplier_id] == factory.DEFAULT_ENERGY_SUPPLIER_ID
        )
        assert (
            actual_row[Colname.balance_responsible_id]
            == factory.DEFAULT_BALANCE_RESPONSIBLE_ID
        )

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
        row = factory.create_row(from_date=from_date, to_date=to_date)
        mock_calculation_input_reader.read_metering_point_periods.return_value = (
            factory.create(spark, row)
        )

        # Act
        actual = get_metering_point_periods_df(
            mock_calculation_input_reader,
            period_start,
            period_end,
            [factory.DEFAULT_GRID_AREA],
        )

        # Assert
        actual_rows = actual.collect()
        assert len(actual_rows) == 1
        assert actual_rows[0][Colname.from_date] == expected_from_date
        assert actual_rows[0][Colname.to_date] == expected_to_date


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
        row = factory.create_row(
            grid_area=grid_area,
            from_grid_area=from_grid_area,
            to_grid_area=to_grid_area,
        )
        mock_calculation_input_reader.read_metering_point_periods.return_value = (
            factory.create(spark, row)
        )

        # Act
        actual = get_metering_point_periods_df(
            mock_calculation_input_reader,
            factory.DEFAULT_FROM_DATE,
            factory.DEFAULT_TO_DATE,
            [calculation_grid_area],
        )

        # Assert
        assert len(actual.collect()) == expected
