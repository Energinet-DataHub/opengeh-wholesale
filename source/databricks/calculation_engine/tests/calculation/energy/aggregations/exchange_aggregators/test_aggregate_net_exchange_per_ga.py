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

from package.calculation.energy.aggregators.exchange_aggregators import (
    aggregate_net_exchange_per_ga,
)
import tests.calculation.energy.quarterly_metering_point_time_series_factories as factories
from package.codelists import QuantityQuality
from package.constants import Colname


class TestWhenValidInput:
    @pytest.mark.parametrize(
        "from_ga_qualities, to_ga_qualities, expected_qualities",
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
        default_grid_area = "111"
        rows = [
            *[
                factories.create_to_row(quality=quality, grid_area=default_grid_area)
                for quality in to_ga_qualities
            ],
            *[
                factories.create_from_row(quality=quality, grid_area=default_grid_area)
                for quality in from_ga_qualities
            ],
        ]
        metering_point_time_series = factories.create(spark, rows)
        expected_qualities = sorted([q.value for q in expected_qualities])

        # Act
        actual = aggregate_net_exchange_per_ga(
            metering_point_time_series, [default_grid_area]
        )

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
        grid_area_1 = "123"  # this is the grid area of the metering point
        grid_area_2 = "234"
        grid_area_3 = "345"
        all_grid_areas = [grid_area_1, grid_area_2, grid_area_3]
        rows = [
            *[
                factories.create_to_row(
                    grid_area=grid_area_1,
                    from_grid_area=grid_area_3,
                    to_grid_area=grid_area_2,
                ),
                factories.create_to_row(
                    grid_area=grid_area_1,
                    from_grid_area=grid_area_2,
                    to_grid_area=grid_area_3,
                ),
            ],
        ]
        metering_point_time_series = factories.create(spark, rows)

        # Act
        actual = aggregate_net_exchange_per_ga(
            metering_point_time_series, all_grid_areas
        )

        # Assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 2
        assert actual_rows[0][Colname.grid_area] == grid_area_2
        assert actual_rows[1][Colname.grid_area] == grid_area_3


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
                    from_grid_area=selected_grid_area,
                    to_grid_area=not_selected_grid_area,
                ),
                factories.create_to_row(
                    grid_area=selected_grid_area,
                    from_grid_area=not_selected_grid_area,
                    to_grid_area=selected_grid_area,
                ),
            ],
        ]
        metering_point_time_series = factories.create(spark, rows)

        # Act
        actual = aggregate_net_exchange_per_ga(
            metering_point_time_series, [selected_grid_area]
        )

        # Assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 1
        assert actual_rows[0][Colname.grid_area] == selected_grid_area
