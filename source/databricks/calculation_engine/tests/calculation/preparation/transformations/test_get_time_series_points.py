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
from datetime import datetime
from decimal import Decimal
from unittest.mock import patch, Mock

import pytest
from pyspark.sql import SparkSession, Row
from pyspark.sql.functions import lit

from package.calculation import input
from package.calculation.input import TableReader
from package.calculation.input.schemas import (
    time_series_point_schema,
)
from package.calculation.preparation.transformations import get_time_series_points
from package.constants import Colname
from tests.helpers.data_frame_utils import assert_dataframes_equal

DEFAULT_OBSERVATION_TIME = datetime(2022, 6, 8, 22, 0, 0)
DEFAULT_FROM_DATE = datetime(2022, 6, 8, 22, 0, 0)
DEFAULT_TO_DATE = datetime(2022, 6, 9, 22, 0, 0)


def _create_time_series_point_row(
    metering_point_id: str = "some-metering-point-id",
    observation_time: datetime = DEFAULT_OBSERVATION_TIME,
) -> Row:
    return {
        Colname.metering_point_id: metering_point_id,
        Colname.quantity: Decimal("1.123456"),
        Colname.quality: "foo",
        Colname.observation_time: observation_time,
    }
    return Row(**row)


def _create_grid_loss_metering_point_row(
    metering_point_id: str = "a-grid-loss-metering-point-id",
) -> Row:
    row = {
        Colname.metering_point_id: metering_point_id,
    }
    return Row(**row)


class TestWhenValidInput:
    @patch.object(input, TableReader.__name__)
    def test_returns_expected_df(
        self,
        mock_calculation_input_reader: Mock,
        spark: SparkSession,
    ) -> None:
        # Arrange
        time_series_row = _create_time_series_point_row()
        expected = spark.createDataFrame(
            data=[time_series_row], schema=time_series_point_schema
        )
        mock_calculation_input_reader.read_time_series_points.return_value = expected

        # Act
        actual = get_time_series_points(
            mock_calculation_input_reader, DEFAULT_FROM_DATE, DEFAULT_TO_DATE
        )

        # Assert
        assert_dataframes_equal(actual, expected)

    @patch.object(input, TableReader.__name__)
    @pytest.mark.parametrize(
        "observation_time, expected",
        [
            (datetime(2022, 6, 8, 22, 0, 0), 1),
            (datetime(2022, 6, 8, 21, 0, 0), 0),
            (datetime(2022, 6, 9, 22, 0, 0), 0),
            (datetime(2022, 6, 9, 21, 0, 0), 1),
        ],
    )
    def test_returns_time_series_df_with_observation_time_within_period_start_and_period_end(
        self,
        mock_calculation_input_reader: Mock,
        spark: SparkSession,
        observation_time: datetime,
        expected: int,
    ) -> None:
        # Arrange
        time_series_row = _create_time_series_point_row(
            observation_time=observation_time
        )

        time_series_points_df = spark.createDataFrame(
            data=[time_series_row],
            schema=time_series_point_schema,
        )
        mock_calculation_input_reader.read_time_series_points.return_value = (
            time_series_points_df
        )

        # Act
        actual = get_time_series_points(
            mock_calculation_input_reader, DEFAULT_FROM_DATE, DEFAULT_TO_DATE
        )

        # Assert
        assert actual.count() == expected

    @patch.object(input, TableReader.__name__)
    @pytest.mark.parametrize(
        "column_name, expected",
        [("observation_year", True), ("observation_month", True), ("dummy", False)],
    )
    def test_returns_time_series_df_without_respective_column(
        self,
        mock_calculation_input_reader: Mock,
        spark: SparkSession,
        column_name: str,
        expected: bool,
    ) -> None:
        # Arrange
        time_series_row = _create_time_series_point_row(
            metering_point_id=str(uuid.uuid4())
        )
        dataframe = spark.createDataFrame(
            data=[time_series_row],
            schema=time_series_point_schema,
        )
        dataframe = dataframe.withColumn(column_name, lit(column_name))
        mock_calculation_input_reader.read_time_series_points.return_value = dataframe

        # Act
        actual = get_time_series_points(
            mock_calculation_input_reader, DEFAULT_FROM_DATE, DEFAULT_TO_DATE
        )

        # Assert
        assert (column_name not in actual.columns) == expected
