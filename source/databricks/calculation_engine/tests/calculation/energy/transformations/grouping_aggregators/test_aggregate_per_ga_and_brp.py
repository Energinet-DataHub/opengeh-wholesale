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

import datetime

from pyspark.sql import SparkSession

from package.calculation.energy.transformations.grouping_aggregators import (
    aggregate_per_ga_and_brp,
)
import tests.calculation.energy.energy_results_factories as factories
from package.codelists import MeteringPointType
from package.constants import Colname


ONE_TIME = datetime.datetime.now()
ANOTHER_TIME = ONE_TIME + datetime.timedelta(minutes=15)
ANY_METERING_POINT_TYPE = MeteringPointType.CONSUMPTION


class TestWhenValidInput:
    def test_returns_values_aggregated_for_ga(self, spark: SparkSession):
        # Arrange
        row = factories.create_row()
        df = factories.create(spark, [row, row])

        # Act
        actual = aggregate_per_ga_and_brp(df, ANY_METERING_POINT_TYPE)

        # assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 1
        actual_row = actual_rows[0]
        assert actual_row[Colname.grid_area] == factories.DEFAULT_GRID_AREA
        assert actual_row[Colname.sum_quantity] == 2 * factories.DEFAULT_SUM_QUANTITY
        assert actual_row[Colname.qualities] == [
            q.value for q in factories.DEFAULT_QUALITIES
        ]
        assert actual_row[Colname.metering_point_type] == ANY_METERING_POINT_TYPE.value
        assert actual_row[Colname.settlement_method] is None  # TODO BJM: Correct?
        assert actual_row[Colname.to_grid_area] is None  # TODO BJM: Correct?
        assert actual_row[Colname.from_grid_area] is None  # TODO BJM: Correct?
        assert actual_row[Colname.energy_supplier_id] is None
        assert (
            actual_row[Colname.balance_responsible_id]
            == factories.DEFAULT_BALANCE_RESPONSIBLE_PARTY_ID
        )
        assert (
            actual_row[Colname.time_window][Colname.start]
            == factories.DEFAULT_OBSERVATION_TIME
        )

    def test_returns_rows_for_each_ga(self, spark: SparkSession):
        # Arrange
        rows = [factories.create_row(), factories.create_row(grid_area="another-ga")]
        df = factories.create(spark, rows)

        # Act
        actual = aggregate_per_ga_and_brp(df, ANY_METERING_POINT_TYPE)

        # assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 2

    def test_returns_rows_for_each_time_window(self, spark: SparkSession):
        # Arrange
        another_time = factories.DEFAULT_OBSERVATION_TIME + datetime.timedelta(
            minutes=15
        )
        rows = [
            factories.create_row(),
            factories.create_row(observation_time=another_time),
        ]
        df = factories.create(spark, rows)

        # Act
        actual = aggregate_per_ga_and_brp(df, ANY_METERING_POINT_TYPE)

        # assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 2

    def test_returns_rows_for_each_brp(self, spark: SparkSession):
        # Arrange
        rows = [
            factories.create_row(),
            factories.create_row(balance_responsible_party_id="another_brp"),
        ]
        df = factories.create(spark, rows)

        # Act
        actual = aggregate_per_ga_and_brp(df, ANY_METERING_POINT_TYPE)

        # assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 2
