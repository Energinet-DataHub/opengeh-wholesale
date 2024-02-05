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
from unittest.mock import patch, Mock


from pyspark.sql import SparkSession

from package import calculation_input
from package.calculation.preparation.transformations import get_time_series_points
from package.calculation_input.table_reader import TableReader
from package.calculation_input.schemas import (
    time_series_point_schema,
    grid_loss_metering_points_schema,
)
from package.constants import Colname
from tests.helpers.data_frame_utils import assert_dataframes_equal

DEFAULT_OBSERVATION_TIME = datetime(2022, 6, 8, 22, 0, 0)
DEFAULT_FROM_DATE = datetime(2022, 6, 8, 22, 0, 0)
DEFAULT_TO_DATE = datetime(2022, 6, 9, 22, 0, 0)


def _create_time_series_point_row(
    metering_point_id: str = "some-metering-point-id",
) -> dict:
    return {
        Colname.metering_point_id: metering_point_id,
        Colname.quantity: Decimal("1.123456"),
        Colname.quality: "foo",
        Colname.observation_time: DEFAULT_OBSERVATION_TIME,
    }


def _create_grid_loss_metering_point_row(
    metering_point_id: str = "a-grid-loss-metering-point-id",
) -> dict:
    return {
        Colname.metering_point_id: metering_point_id,
    }


class TestWhenValidInput:
    @patch.object(calculation_input, TableReader.__name__)
    def test_returns_expected_df(
        self,
        mock_calculation_input_reader: Mock,
        spark: SparkSession,
    ) -> None:
        # Arrange
        row = _create_time_series_point_row()
        time_series_points_df = spark.createDataFrame(
            data=[row], schema=time_series_point_schema
        )
        mock_calculation_input_reader.read_time_series_points.return_value = (
            time_series_points_df
        )
        expected = time_series_points_df

        grid_loss_metering_points = spark.createDataFrame(
            data=[], schema=grid_loss_metering_points_schema
        )
        mock_calculation_input_reader.read_grid_loss_metering_points.return_value = (
            grid_loss_metering_points
        )

        # Act
        actual = get_time_series_points(
            mock_calculation_input_reader, DEFAULT_FROM_DATE, DEFAULT_TO_DATE
        )

        # Assert
        assert_dataframes_equal(actual, expected)

    @patch.object(calculation_input, TableReader.__name__)
    def test_returns_df_without_time_series_of_grid_loss_metering_points(
        self,
        mock_calculation_input_reader: Mock,
        spark: SparkSession,
    ) -> None:
        # Arrange
        non_grid_loss_time_series_row = _create_time_series_point_row(
            metering_point_id="non-grid-loss-metering-point-id"
        )
        grid_loss_time_series_row = _create_time_series_point_row(
            metering_point_id="grid-loss-metering-point-id"
        )
        time_series_points_df = spark.createDataFrame(
            data=[grid_loss_time_series_row, non_grid_loss_time_series_row],
            schema=time_series_point_schema,
        )
        mock_calculation_input_reader.read_time_series_points.return_value = (
            time_series_points_df
        )
        grid_loss_metering_point_row = _create_grid_loss_metering_point_row(
            metering_point_id=grid_loss_time_series_row[Colname.metering_point_id]
        )
        grid_loss_metering_points_df = spark.createDataFrame(
            data=[grid_loss_metering_point_row],
            schema=grid_loss_metering_points_schema,
        )
        mock_calculation_input_reader.read_grid_loss_metering_points.return_value = (
            grid_loss_metering_points_df
        )

        # Act
        actual = get_time_series_points(
            mock_calculation_input_reader, DEFAULT_FROM_DATE, DEFAULT_TO_DATE
        )

        # Assert: That only the non-grid-loss time series is returned
        assert actual.count() == 1
        assert (
            actual.collect()[0][Colname.metering_point_id]
            == non_grid_loss_time_series_row[Colname.metering_point_id]
        )
