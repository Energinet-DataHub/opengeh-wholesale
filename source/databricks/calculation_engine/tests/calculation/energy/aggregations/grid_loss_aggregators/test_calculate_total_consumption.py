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

import pytest
from pyspark.sql import SparkSession

from package.calculation.energy.aggregators.grid_loss_aggregators import (
    calculate_total_consumption,
)
from package.codelists import QuantityQuality
from package.constants import Colname

from tests.calculation.energy import energy_results_factories as energy_results


class TestWhenValidInput:
    @pytest.mark.parametrize(
        "prod_qualities, exchange_qualities, expected_qualities",
        [
            (
                [QuantityQuality.MEASURED],
                [QuantityQuality.CALCULATED],
                [QuantityQuality.MEASURED.value, QuantityQuality.CALCULATED.value],
            ),
            (
                [QuantityQuality.MEASURED, QuantityQuality.CALCULATED],
                [QuantityQuality.MEASURED, QuantityQuality.CALCULATED],
                [QuantityQuality.MEASURED.value, QuantityQuality.CALCULATED.value],
            ),
        ],
    )
    def test__returns_distinct_qualities_from_production_and_exchange_from_neighbor_ga(
        self,
        spark: SparkSession,
        prod_qualities: list[QuantityQuality],
        exchange_qualities: list[QuantityQuality],
        expected_qualities: list[str],
    ) -> None:
        # Arrange
        production = energy_results.create_row(qualities=prod_qualities)
        exchange = energy_results.create_row(qualities=exchange_qualities)
        production_per_ga = energy_results.create(spark, production)
        exchange_per_ga = energy_results.create(spark, exchange)

        # Act
        actual = calculate_total_consumption(production_per_ga, exchange_per_ga)

        # Assert
        actual_row = actual.df.collect()[0]
        assert sorted(actual_row[Colname.qualities]) == sorted(expected_qualities)

    def test__does_not_include_qualities_from_non_neighbor_ga_in_return(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        production = energy_results.create_row(qualities=[QuantityQuality.MEASURED])
        exchange_other_ga = [
            energy_results.create_row(qualities=[QuantityQuality.ESTIMATED]),
            energy_results.create_row(
                qualities=[QuantityQuality.CALCULATED], grid_area="some-other-grid-area"
            ),
        ]
        production_per_ga = energy_results.create(spark, production)
        exchange_per_ga = energy_results.create(spark, exchange_other_ga)

        # Act
        actual = calculate_total_consumption(production_per_ga, exchange_per_ga)

        # Assert
        actual_row = actual.df.collect()[0]
        assert sorted(actual_row[Colname.qualities]) == sorted(
            [QuantityQuality.MEASURED.value, QuantityQuality.ESTIMATED.value]
        )

    def test__returns_production_and_exchange_from_neighbor_ga(
        self, spark: SparkSession
    ):
        # Arrange
        production = [
            energy_results.create_row(quantity=1),
            energy_results.create_row(quantity=2),
        ]
        exchange = [
            energy_results.create_row(quantity=4),
            energy_results.create_row(quantity=8, grid_area="some-other-grid-area"),
        ]
        production_per_ga = energy_results.create(spark, production)
        exchange_per_ga = energy_results.create(spark, exchange)
        # The sum of production and exchange, but not including exchange for the other grid area
        expected_quantity = 7

        # Act
        actual = calculate_total_consumption(production_per_ga, exchange_per_ga)

        # Assert
        actual_row = actual.df.collect()[0]
        assert actual_row[Colname.quantity] == expected_quantity

    def test__does_not_include_quantity_from_non_neighbor_ga_in_return(
        self, spark: SparkSession
    ):
        # Arrange
        production = [
            energy_results.create_row(quantity=1),
            energy_results.create_row(quantity=2),
        ]
        exchange_other_ga = [
            energy_results.create_row(quantity=4),
            energy_results.create_row(quantity=8, grid_area="some-other-grid-area"),
        ]
        production_per_ga = energy_results.create(spark, production)
        exchange_per_ga = energy_results.create(spark, exchange_other_ga)
        # The sum of production and exchange, but not including exchange for the other grid area
        expected_quantity = 7

        # Act
        actual = calculate_total_consumption(production_per_ga, exchange_per_ga)

        # Assert
        actual_row = actual.df.collect()[0]
        assert actual_row[Colname.quantity] == expected_quantity
