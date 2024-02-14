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

import pytest

from calculator_job.test_data_builder import TableReaderMockBuilder
from package.calculation.calculation_execute import calculation_execute
from package.calculation.calculator_args import CalculatorArgs


@pytest.fixture
def setup(
    any_calculator_args: CalculatorArgs,
):

    # Arrange
    any_calculator_args.calculation_grid_areas = ["805"]
    any_calculator_args.calculation_period_start_datetime = datetime(
        2019, 12, 30, 23, 0, 0
    )
    any_calculator_args.calculation_period_end_datetime = datetime(2020, 1, 1, 23, 0, 0)
    return any_calculator_args


"""
Period_Start                               Period_Start
 2019-12-31            2020-01-01           2020-01-02
     |---------------------|---------------------|
MPP1 (E18)                 |---------------------|
MPP2 (E17)                 |---------------------|
TSP                        x
"""


def test_demo(
    setup,
    spark,
):

    builder = TableReaderMockBuilder(
        spark, "features/simple_wholesale_fixing/test_data/"
    )

    builder.populate_metering_point_periods("metering_point_periods.csv")
    builder.populate_time_series_points("time_series_points.csv")
    builder.populate_grid_loss_metering_points("grid_loss_metering_points.csv")
    prepared_data_reader = builder.create_prepared_data_reader()

    # Act
    actual = calculation_execute(setup, prepared_data_reader)

    # Assert
    assert actual.basis_data.metering_point_time_series.count() == 48
    assert actual.basis_data.metering_point_periods.count() == 2
