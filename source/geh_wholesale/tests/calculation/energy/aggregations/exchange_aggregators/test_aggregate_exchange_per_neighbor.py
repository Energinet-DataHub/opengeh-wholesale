import pytest
from pyspark.sql import SparkSession

import tests.calculation.energy.metering_point_time_series_factories as factories
from geh_wholesale.calculation.energy.aggregators.exchange_aggregators import (
    aggregate_exchange_per_neighbor,
)
from geh_wholesale.codelists import QuantityQuality
from geh_wholesale.constants import Colname


class TestWhenValidInput:
    @pytest.mark.parametrize(
        ("from_ga_qualities", "to_ga_qualities", "expected_qualities"),
        [
            # Case where no "from" metering point exists
            ([], [QuantityQuality.ESTIMATED], [QuantityQuality.ESTIMATED]),
            # Case where no "to" metering point exists
            ([QuantityQuality.ESTIMATED], [], [QuantityQuality.ESTIMATED]),
            (
                [QuantityQuality.ESTIMATED],
                [QuantityQuality.ESTIMATED],
                [QuantityQuality.ESTIMATED],
            ),
            (
                [QuantityQuality.ESTIMATED, QuantityQuality.ESTIMATED],
                [],
                [QuantityQuality.ESTIMATED],
            ),
            (
                [QuantityQuality.ESTIMATED, QuantityQuality.MISSING],
                [QuantityQuality.ESTIMATED, QuantityQuality.CALCULATED],
                [
                    QuantityQuality.CALCULATED,
                    QuantityQuality.ESTIMATED,
                    QuantityQuality.MISSING,
                ],
            ),
        ],
    )
    def test_returns_distinct_qualities_from_both_to_and_from_ga(
        self,
        spark: SparkSession,
        from_ga_qualities: list[QuantityQuality],
        to_ga_qualities: list[QuantityQuality],
        expected_qualities: list[QuantityQuality],
    ) -> None:
        # Arrange
        from_grid_area = "111"
        to_grid_area = "222"
        rows = [
            *[
                factories.create_to_row(
                    quality=quality,
                    grid_area=to_grid_area,
                    from_grid_area=from_grid_area,
                )
                for quality in to_ga_qualities
            ],
            *[
                factories.create_from_row(
                    quality=quality,
                    grid_area=from_grid_area,
                    to_grid_area=to_grid_area,
                )
                for quality in from_ga_qualities
            ],
        ]
        metering_point_time_series = factories.create(spark, rows)
        expected_qualities = sorted([q.value for q in expected_qualities])

        # Act
        actual = aggregate_exchange_per_neighbor(metering_point_time_series, [from_grid_area, to_grid_area])

        # Assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 2
        assert sorted(actual_rows[0][Colname.qualities]) == expected_qualities
        assert sorted(actual_rows[1][Colname.qualities]) == expected_qualities


class TestWhenInputHasDataNotBelongingToSelectedGridArea:
    def test_returns_result_only_for_selected_grid_area(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        selected_grid_area = "234"
        not_selected_grid_area = "345"
        rows = [
            *[
                factories.create_to_row(
                    grid_area=selected_grid_area,
                    from_grid_area=not_selected_grid_area,
                ),
                factories.create_to_row(
                    grid_area=not_selected_grid_area,
                    from_grid_area=selected_grid_area,
                ),
            ],
        ]
        metering_point_time_series = factories.create(spark, rows)

        # Act
        actual = aggregate_exchange_per_neighbor(metering_point_time_series, [selected_grid_area])

        # Assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 1
        assert actual_rows[0][Colname.grid_area_code] == selected_grid_area
