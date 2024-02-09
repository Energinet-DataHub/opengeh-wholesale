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
import os
from datetime import datetime
from unittest.mock import patch, Mock
from calculation_input.factories import raw_grid_loss_metering_points_factory
from calculator_job.test_data_builder import TableReaderMockBuilder
from package import calculation_input
from package.calculation import PreparedDataReader
from package.calculation.calculation_execute import calculation_execute
from package.calculation.calculator_args import CalculatorArgs
from package.calculation_input import TableReader
from package.calculation_input.schemas import metering_point_period_schema
from package.codelists import InputMeteringPointType
import calculation_input.factories.raw_time_series_point_factory \
    as raw_time_series_point_factory
import calculation_input.factories.input_metering_point_periods_factory \
    as raw_metering_point_periods_factory


class TestBusinessLogic:
    @patch.object(calculation_input, TableReader.__name__)
    def test1(
        self,
        mock_table_reader: Mock,
            args: CalculatorArgs,
        spark,
    ):
        """
        asdasd
        |-------------|asdasd
        Args:
            mock_table_reader ():
            calculator_args ():
            spark ():

        Returns:

        """
        # Arrange
        args.calculation_grid_areas = ["805"]
        args.calculation_period_start_datetime = datetime(2019, 12, 30, 23, 0, 0)
        args.calculation_period_end_datetime = datetime(2020, 1, 1, 23, 0, 0)

        row1 = raw_metering_point_periods_factory.create_row()
        row2 = raw_metering_point_periods_factory.create_row(
            metering_point_id="123456789012345678901234568",
            grid_area="805",
            metering_point_type=InputMeteringPointType.CONSUMPTION
        )

        raw_metering_point_periods_factory.create_dataframe(spark, [row1, row2]).show()

        mock_table_reader.read_metering_point_periods.return_value = (
            raw_metering_point_periods_factory.create_dataframe(spark, [row1, row2])
        )

        mock_table_reader.read_time_series_points.return_value = (
            raw_time_series_point_factory.create_dataframe(spark)
        )

        row3 = raw_grid_loss_metering_points_factory.create_row()
        row4 = raw_grid_loss_metering_points_factory.create_row(
            metering_point_id="123456789012345678901234568"
        )
        mock_table_reader.read_grid_loss_metering_points.return_value = (
            raw_grid_loss_metering_points_factory.create_dataframe(spark, [row3, row4])
        )

        prepared_data_reader: PreparedDataReader = PreparedDataReader(
            mock_table_reader
        )

        # Act
        actual = calculation_execute(args, prepared_data_reader)

        # Assert
        actual.basis_data.metering_point_time_series.count = 2


    @patch.object(calculation_input, TableReader.__name__)
    def test2(
            self,
            args: CalculatorArgs,
            spark,
    ):
        # Arrange
        args.calculation_grid_areas = ["805"]
        args.calculation_period_start_datetime = datetime(2019, 12, 30, 23, 0, 0)
        args.calculation_period_end_datetime = datetime(2020, 1, 1, 23, 0, 0)

        builder = TableReaderMockBuilder(spark)
        builder.populate_metering_point_periods("metering_point_periods.csv")

        builder.get_table_reader().read_time_series_points.return_value = (
            raw_time_series_point_factory.create_dataframe(spark)
        )

        row3 = raw_grid_loss_metering_points_factory.create_row()
        row4 = raw_grid_loss_metering_points_factory.create_row(
            metering_point_id="123456789012345678901234568"
        )
        builder.get_table_reader().read_grid_loss_metering_points.return_value = (
            raw_grid_loss_metering_points_factory.create_dataframe(spark, [row3, row4])
        )

        prepared_data_reader: PreparedDataReader = PreparedDataReader(
            builder.get_table_reader()
        )

        # Act
        actual = calculation_execute(args, prepared_data_reader)

        # Assert
        actual.basis_data.metering_point_time_series.count = 2
