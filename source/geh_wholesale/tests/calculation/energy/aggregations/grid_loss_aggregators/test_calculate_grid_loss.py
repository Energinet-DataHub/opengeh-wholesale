import decimal

import pytest
from pyspark.sql import SparkSession

import tests.calculation.energy.energy_results_factories as factories
from geh_wholesale.calculation.energy.aggregators.grid_loss_aggregators import (
    calculate_grid_loss,
)
from geh_wholesale.codelists import QuantityQuality
from geh_wholesale.constants import Colname


class TestWhenValidInput:
    @pytest.mark.parametrize(
        (
            "exchange_qualities",
            "non_profiled_consumption_qualities",
            "flex_consumption_qualities",
            "production_qualities",
        ),
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
        exchange_qualities: list[QuantityQuality],
        non_profiled_consumption_qualities: list[QuantityQuality],
        flex_consumption_qualities: list[QuantityQuality],
        production_qualities: list[QuantityQuality],
    ) -> None:
        """
        According to SME there is always a net exchange per grid area point for any given
        observation time. Thus, there is no test case where net exchange does not exist.
        """

        # Arrange
        exchange = self._create_energy_results(spark, exchange_qualities)
        non_profiled = self._create_energy_results(spark, non_profiled_consumption_qualities)
        flex = self._create_energy_results(spark, flex_consumption_qualities)
        production = self._create_energy_results(spark, production_qualities)

        # Act
        actual = calculate_grid_loss(exchange, non_profiled, flex, production)

        # Assert
        actual_row = actual.df.collect()[0]
        assert sorted(actual_row[Colname.qualities]) == [QuantityQuality.CALCULATED.value]

    @staticmethod
    def _create_energy_results(spark, qualities):
        """Create an energy results data frame with a row for each quality in qualities."""
        return factories.create(
            spark,
            [factories.create_row(qualities=quality) for quality in qualities],
        )


class TestWhenEnergyResultsIsEmpty:
    @pytest.mark.parametrize(
        ("exchange", "non_profiled_consumption", "flex_consumption", "production", "expected_quantity"),
        [
            (  # Empty non profiled
                factories.create_row(quantity=400),
                [],
                factories.create_row(quantity=200),
                factories.create_row(quantity=100),
                300,
            ),
            (  # Empty flex
                factories.create_row(quantity=400),
                factories.create_row(quantity=300),
                [],
                factories.create_row(quantity=100),
                200,
            ),
            (  # Empty production
                factories.create_row(quantity=400),
                factories.create_row(quantity=300),
                factories.create_row(quantity=200),
                [],
                -100,
            ),
            (  # Empty non profiled and flex
                factories.create_row(quantity=400),
                [],
                [],
                factories.create_row(quantity=100),
                500,
            ),
            (  # Empty flex and production
                factories.create_row(quantity=400),
                factories.create_row(quantity=300),
                [],
                [],
                100,
            ),
            (  # Empty non-profiled and production
                factories.create_row(quantity=400),
                [],
                factories.create_row(quantity=200),
                [],
                200,
            ),
            (  # Empty exchange
                [],
                factories.create_row(quantity=300),
                factories.create_row(quantity=200),
                factories.create_row(quantity=100),
                -400,
            ),
            (  # Empty exchange and non profiled
                [],
                [],
                factories.create_row(quantity=200),
                factories.create_row(quantity=100),
                -100,
            ),
            (  # Empty exchange and flex
                [],
                factories.create_row(quantity=300),
                [],
                factories.create_row(quantity=100),
                -200,
            ),
            (  # Empty exchange and production
                [],
                factories.create_row(quantity=300),
                factories.create_row(quantity=200),
                [],
                -500,
            ),
        ],
    )
    def test_returns_correct_quantity(
        self,
        spark: SparkSession,
        exchange: factories.Row,
        non_profiled_consumption: factories.Row,
        flex_consumption: factories.Row,
        production: factories.Row,
        expected_quantity: decimal,
    ) -> None:
        # Arrange
        exchange = factories.create(spark, exchange)
        non_profiled = factories.create(spark, non_profiled_consumption)
        flex = factories.create(spark, flex_consumption)
        production = factories.create(spark, production)

        # Act
        actual = calculate_grid_loss(exchange, non_profiled, flex, production)

        # Assert
        assert actual.df.collect()[0][Colname.quantity] == expected_quantity
