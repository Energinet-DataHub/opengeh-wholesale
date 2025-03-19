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

import tests.calculation.energy.energy_results_factories as factories
from geh_wholesale.calculation.energy.aggregators.grouping_aggregators import (
    aggregate,
)
from geh_wholesale.codelists import MeteringPointType
from geh_wholesale.constants import Colname

ONE_TIME = datetime.datetime.now()
ANOTHER_TIME = ONE_TIME + datetime.timedelta(minutes=15)
ANY_METERING_POINT_TYPE = MeteringPointType.CONSUMPTION


class TestWhenValidInput:
    def test_returns_values_aggregated_for_ga(self, spark: SparkSession):
        # Arrange
        row = factories.create_row()
        energy_results = factories.create(spark, [row, row])

        # Act
        actual = aggregate(energy_results)

        # assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 1
        actual_row = actual_rows[0]
        assert actual_row[Colname.grid_area_code] == factories.DEFAULT_GRID_AREA
        assert actual_row[Colname.quantity] == 2 * factories.DEFAULT_QUANTITY
        assert actual_row[Colname.qualities] == [q.value for q in factories.DEFAULT_QUALITIES]
        assert actual_row[Colname.to_grid_area_code] is None  # None because it's not an exchange result
        assert actual_row[Colname.from_grid_area_code] is None  # None because it's not an exchange result
        assert actual_row[Colname.energy_supplier_id] is None
        assert actual_row[Colname.balance_responsible_party_id] is None
        assert actual_row[Colname.observation_time] == factories.DEFAULT_OBSERVATION_TIME

    def test_returns_rows_for_each_ga(self, spark: SparkSession):
        # Arrange
        rows = [factories.create_row(), factories.create_row(grid_area="another-ga")]
        df = factories.create(spark, rows)

        # Act
        actual = aggregate(df)

        # assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 2

    def test_returns_rows_for_each_observation_time(self, spark: SparkSession):
        # Arrange
        another_time = factories.DEFAULT_OBSERVATION_TIME + datetime.timedelta(minutes=15)
        rows = [
            factories.create_row(),
            factories.create_row(observation_time=another_time),
        ]
        df = factories.create(spark, rows)

        # Act
        actual = aggregate(df)

        # assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 2
