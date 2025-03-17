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
from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession
from pyspark.sql.functions import lit

import tests.calculation.energy.energy_results_factories as energy_results_factory
import tests.calculation.energy.metering_point_time_series_factories as metering_point_time_series_factory
from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.calculation.energy.calculated_grid_loss import (
    append_calculated_grid_loss_to_metering_point_times_series,
)
from geh_wholesale.calculation.preparation.data_structures import (
    PreparedMeteringPointTimeSeries,
)
from geh_wholesale.codelists import QuantityQuality
from geh_wholesale.constants import Colname


@pytest.mark.parametrize(
    "calculation_period_end_datetime, expected_resolution",
    [
        (datetime(2023, 4, 1), "PT1H"),
        (datetime(2023, 5, 1), "PT1H"),
        (datetime(2023, 6, 1), "PT15M"),
    ],
)
def test__add_calculated_grid_loss_to_metering_point_times_series__returns_correct_resolution(
    calculation_period_end_datetime: datetime, expected_resolution: str
):
    # Arrange
    spark = SparkSession.builder.getOrCreate()

    mock_calculator_args = Mock(spec=CalculatorArgs)
    mock_calculator_args.quarterly_resolution_transition_datetime = datetime(2023, 5, 1)
    mock_calculator_args.period_end_datetime = calculation_period_end_datetime

    positive_grid_loss_row = energy_results_factory.create_grid_loss_row()
    negative_grid_loss_row = energy_results_factory.create_grid_loss_row()

    positive_grid_loss = energy_results_factory.create(spark, positive_grid_loss_row)
    negative_grid_loss = energy_results_factory.create(spark, negative_grid_loss_row)

    metering_point_time_series = metering_point_time_series_factory.create(spark)
    prepared_metering_point_time_series = PreparedMeteringPointTimeSeries(
        metering_point_time_series.df.withColumn(Colname.resolution, lit("PT15M"))
    )

    # Act
    result = append_calculated_grid_loss_to_metering_point_times_series(
        mock_calculator_args,
        prepared_metering_point_time_series,
        positive_grid_loss,
        negative_grid_loss,
    )

    # Assert
    assert result.df.count() == 3
    positive_and_negative_grid_loss = result.df.filter(result.df[Colname.quality] == QuantityQuality.CALCULATED.value)
    assert positive_and_negative_grid_loss.count() == 2
    assert result.df.collect()[0][Colname.resolution] == expected_resolution
    assert result.df.collect()[1][Colname.resolution] == expected_resolution
