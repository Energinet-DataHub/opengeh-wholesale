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

from pyspark.sql import SparkSession
from datetime import datetime

from package.calculation.energy.transformations import adjust_flex_consumption
from package.codelists import (
    MeteringPointType,
    QuantityQuality,
)
from package.constants import Colname
import tests.calculation.energy.energy_results_factories as energy_results_factories
import tests.calculation.energy.grid_loss_responsible_factories as grid_loss_responsible_factories

# This time should be within the time window of the grid loss responsible
DEFAULT_OBSERVATION_TIME = "2020-01-01T00:00:00.000Z"


class TestWhenValidInput:
    def test_returns_qualities_from_flex_consumption_and_positive_grid_loss(
        self, spark: SparkSession, timestamp_factory
    ) -> None:
        # Arrange
        expected_qualities = [
            QuantityQuality.CALCULATED.value,
            QuantityQuality.ESTIMATED.value,
        ]

        flex_consumption_row = energy_results_factories.create_row(
            observation_time=timestamp_factory(DEFAULT_OBSERVATION_TIME),
            energy_supplier_id="energy_supplier_id",
            qualities=[QuantityQuality.CALCULATED],
            metering_point_type=MeteringPointType.CONSUMPTION,
            sum_quantity=10,
        )
        flex_consumption = energy_results_factories.create(
            spark, [flex_consumption_row]
        )

        positive_grid_loss_row = energy_results_factories.create_row(
            observation_time=timestamp_factory(DEFAULT_OBSERVATION_TIME),
            qualities=[QuantityQuality.ESTIMATED],
            metering_point_type=MeteringPointType.CONSUMPTION,
            sum_quantity=20,
        )
        positive_grid_loss = energy_results_factories.create(
            spark, [positive_grid_loss_row]
        )

        grid_loss_responsible_row = grid_loss_responsible_factories.create_row(
            energy_supplier_id="energy_supplier_id",
            metering_point_type=MeteringPointType.CONSUMPTION,
            is_positive_grid_loss_responsible=True,
        )
        grid_loss_responsible = grid_loss_responsible_factories.create(
            spark, [grid_loss_responsible_row]
        )

        # Act
        actual = adjust_flex_consumption(
            flex_consumption, positive_grid_loss, grid_loss_responsible
        )

        # Assert
        actual_row = actual.df.collect()[0]
        actual_qualities = actual_row[Colname.qualities]
        assert set(actual_qualities) == set(expected_qualities)
        assert actual_row[Colname.sum_quantity] == 30


class TestWhenNoFlexConsumption:
    def test_returns_only_positive_grid_loss(
        self, spark: SparkSession, timestamp_factory
    ) -> None:
        # Arrange
        flex_consumption = energy_results_factories.create(spark, [])

        positive_grid_loss_rows = [
            energy_results_factories.create_row(
                observation_time=timestamp_factory(DEFAULT_OBSERVATION_TIME),
                metering_point_type=MeteringPointType.CONSUMPTION,
                sum_quantity=20,
            )
        ]
        positive_grid_loss = energy_results_factories.create(
            spark, positive_grid_loss_rows
        )

        grid_loss_responsible_row = grid_loss_responsible_factories.create_row(
            metering_point_type=MeteringPointType.CONSUMPTION,
            is_positive_grid_loss_responsible=True,
        )
        grid_loss_responsible = grid_loss_responsible_factories.create(
            spark, [grid_loss_responsible_row]
        )

        # Act
        actual = adjust_flex_consumption(
            flex_consumption, positive_grid_loss, grid_loss_responsible
        )

        # Assert
        assert actual.df.count() == 1
        assert actual.df.collect()[0][Colname.sum_quantity] == 20
