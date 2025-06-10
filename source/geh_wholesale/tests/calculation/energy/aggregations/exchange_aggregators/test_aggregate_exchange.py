import pytest
from pyspark.sql import SparkSession

import tests.calculation.energy.metering_point_time_series_factories as factories
from geh_wholesale.calculation.energy.aggregators.exchange_aggregators import (
    aggregate_exchange,
    aggregate_exchange_per_neighbor,
)
from geh_wholesale.codelists import QuantityQuality
from geh_wholesale.constants import Colname


class TestWhenValidInput:
    @pytest.mark.parametrize(
        ("from_qualities", "to_qualities", "expected_qualities"),
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
        from_qualities: list[QuantityQuality],
        to_qualities: list[QuantityQuality],
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
                for quality in to_qualities
            ],
            *[
                factories.create_from_row(
                    quality=quality,
                    grid_area=from_grid_area,
                    to_grid_area=to_grid_area,
                )
                for quality in from_qualities
            ],
        ]
        metering_point_time_series = factories.create(spark, rows)
        expected_qualities = sorted([q.value for q in expected_qualities])

        # Act
        exchange_per_neighbor = aggregate_exchange_per_neighbor(
            metering_point_time_series, [from_grid_area, to_grid_area]
        )
        actual = aggregate_exchange(exchange_per_neighbor)

        # Assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 2
        assert sorted(actual_rows[0][Colname.qualities]) == expected_qualities
        assert sorted(actual_rows[1][Colname.qualities]) == expected_qualities


class TestWhenMeteringPointIsNeitherInToOrFromGridArea:
    def test_returns_result_with_only_to_and_from_grid_area(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        other_grid_area = "123"  # this is the grid area of the metering point
        exchange_grid_area_1 = "234"
        exchange_grid_area_2 = "345"
        all_grid_areas = [other_grid_area, exchange_grid_area_1, exchange_grid_area_2]
        rows = [
            *[
                factories.create_exchange_row(
                    grid_area=other_grid_area,
                    from_grid_area=exchange_grid_area_2,
                    to_grid_area=exchange_grid_area_1,
                ),
                factories.create_exchange_row(
                    grid_area=other_grid_area,
                    from_grid_area=exchange_grid_area_1,
                    to_grid_area=exchange_grid_area_2,
                ),
            ],
        ]
        metering_point_time_series = factories.create(spark, rows)

        # Act
        exchange_per_neighbor = aggregate_exchange_per_neighbor(metering_point_time_series, all_grid_areas)
        actual = aggregate_exchange(exchange_per_neighbor)

        # Assert
        actual_grid_areas = [row[Colname.grid_area_code] for row in actual.df.collect()]

        assert sorted(actual_grid_areas) == sorted([exchange_grid_area_1, exchange_grid_area_2])


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
        exchange_per_neighbor = aggregate_exchange_per_neighbor(metering_point_time_series, [selected_grid_area])
        actual = aggregate_exchange(exchange_per_neighbor)

        # Assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 1
        assert actual_rows[0][Colname.grid_area_code] == selected_grid_area
