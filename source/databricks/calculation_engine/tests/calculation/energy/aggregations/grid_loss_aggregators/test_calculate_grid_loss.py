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
import decimal

import pytest
from pyspark.sql import SparkSession

import tests.calculation.energy.energy_results_factories as factories
from package.calculation.energy.aggregators.grid_loss_aggregators import (
    calculate_grid_loss,
)
from package.codelists import QuantityQuality
from package.constants import Colname


class TestWhenValidInput:
    @pytest.mark.parametrize(
        "net_exchange_per_ga_qualities, non_profiled_consumption_qualities, flex_consumption_qualities, production_qualities",
        [
            # Quality from all aggregated time series are included in the result
            (
                [QuantityQuality.ESTIMATED, QuantityQuality.MISSING],
                [QuantityQuality.ESTIMATED, QuantityQuality.MISSING],
                [QuantityQuality.MEASURED, QuantityQuality.MISSING],
                [QuantityQuality.CALCULATED, QuantityQuality.MISSING],
            ),
            # Point exists when no non profiled consumption
            (
                [QuantityQuality.MISSING],
                [],
                [QuantityQuality.MEASURED],
                [QuantityQuality.CALCULATED],
            ),
            # Point exists when no flex consumption
            (
                [QuantityQuality.MISSING],
                [QuantityQuality.MEASURED],
                [],
                [QuantityQuality.CALCULATED],
            ),
            # Point exists when no production
            (
                [QuantityQuality.MISSING],
                [QuantityQuality.MEASURED],
                [QuantityQuality.CALCULATED],
                [],
            ),
        ],
    )
    def test_returns_quality_calculated(
        self,
        spark: SparkSession,
        net_exchange_per_ga_qualities: list[QuantityQuality],
        non_profiled_consumption_qualities: list[QuantityQuality],
        flex_consumption_qualities: list[QuantityQuality],
        production_qualities: list[QuantityQuality],
    ) -> None:
        """
        According to SME there is always a net exchange per grid area point for any given
        observation time. Thus, there is no test case where net exchange does not exist.
        """

        # Arrange
        exchange = self._create_energy_results(spark, net_exchange_per_ga_qualities)
        non_profiled = self._create_energy_results(
            spark, non_profiled_consumption_qualities
        )
        flex = self._create_energy_results(spark, flex_consumption_qualities)
        production = self._create_energy_results(spark, production_qualities)

        # Act
        actual = calculate_grid_loss(exchange, non_profiled, flex, production)

        # Assert
        actual_row = actual.df.collect()[0]
        assert sorted(actual_row[Colname.qualities]) == [
            QuantityQuality.CALCULATED.value
        ]

    @staticmethod
    def _create_energy_results(spark, qualities):
        """Create an energy results data frame with a row for each quality in qualities."""
        return factories.create(
            spark,
            [factories.create_row(qualities=quality) for quality in qualities],
        )


class TestWhenEnergyResultsIsEmpty:
    @pytest.mark.parametrize(
        "net_exchange_per_ga, non_profiled_consumption, flex_consumption, production, expected_quantity",
        [
            (  # No empty EnergyResults
                factories.create_row(quantity=100),
                factories.create_row(quantity=100),
                factories.create_row(quantity=100),
                factories.create_row(quantity=100),
                0,
            ),
            (  # Empty net exchange (Not possible according to SME)
                [],
                factories.create_row(quantity=100),
                factories.create_row(quantity=100),
                factories.create_row(quantity=100),
                -100,
            ),
            (  # Empty non profiled
                factories.create_row(quantity=100),
                [],
                factories.create_row(quantity=100),
                factories.create_row(quantity=100),
                100,
            ),
            (  # Empty flex
                factories.create_row(quantity=100),
                factories.create_row(quantity=100),
                [],
                factories.create_row(quantity=100),
                100,
            ),
            (  # Empty production
                factories.create_row(quantity=100),
                factories.create_row(quantity=100),
                factories.create_row(quantity=100),
                [],
                -100,
            ),
            (  # Empty net exchange and production
                [],
                factories.create_row(quantity=100),
                factories.create_row(quantity=100),
                [],
                -200,
            ),
            (  # Empty non profiled and flex
                factories.create_row(quantity=100),
                [],
                [],
                factories.create_row(quantity=100),
                200,
            ),
            (  # Empty flex and production
                factories.create_row(quantity=100),
                factories.create_row(quantity=100),
                [],
                [],
                0,
            ),
            (  # Empty non profiled, flex and production
                factories.create_row(quantity=100),
                [],
                [],
                [],
                100,
            ),
        ],
    )
    def test_returns_correct_quantity(
        self,
        spark: SparkSession,
        net_exchange_per_ga: factories.Row,
        non_profiled_consumption: factories.Row,
        flex_consumption: factories.Row,
        production: factories.Row,
        expected_quantity: decimal,
    ) -> None:
        # Arrange
        exchange = factories.create(spark, net_exchange_per_ga)
        non_profiled = factories.create(spark, non_profiled_consumption)
        flex = factories.create(spark, flex_consumption)
        production = factories.create(spark, production)

        # Act
        actual = calculate_grid_loss(exchange, non_profiled, flex, production)
        actual.df.show(100, False)

        # Assert
        assert actual.df.collect()[0][Colname.quantity] == expected_quantity
