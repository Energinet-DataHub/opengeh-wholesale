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
from datetime import datetime, timedelta
from decimal import Decimal

import pytest
from pyspark.sql import SparkSession
from pyspark.sql.types import Row

from calculation.energy import quarterly_metering_point_time_series_factories
from package.calculation.energy.aggregators.exchange_aggregators import (
    aggregate_net_exchange_per_neighbour_ga,
)
from package.calculation.preparation.data_structures.metering_point_time_series import (
    MeteringPointTimeSeries,
)
from package.codelists import (
    MeteringPointType,
)
from package.constants import Colname

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime(
    "2020-01-01T00:00:00+0000", date_time_formatting_string
)
numberOfTestQuarters = 96

ALL_GRID_AREAS = ["A", "B", "C"]


@pytest.fixture(scope="module")
def single_quarter_test_data(spark: SparkSession) -> MeteringPointTimeSeries:
    rows = [
        _create_row("A", "A", "B", default_obs_time, Decimal("10")),
        _create_row("A", "A", "B", default_obs_time, Decimal("15")),
        _create_row("A", "B", "A", default_obs_time, Decimal("5")),
        _create_row("B", "B", "A", default_obs_time, Decimal("10")),
        _create_row("A", "A", "C", default_obs_time, Decimal("20")),
        _create_row("C", "C", "A", default_obs_time, Decimal("10")),
        _create_row("C", "C", "A", default_obs_time, Decimal("5")),
    ]
    return quarterly_metering_point_time_series_factories.create(spark, rows)


@pytest.fixture(scope="module")
def multi_quarter_test_data(spark: SparkSession) -> MeteringPointTimeSeries:
    rows = []

    for i in range(numberOfTestQuarters):
        obs_time = default_obs_time + timedelta(minutes=15 * i)

        rows.append(_create_row("A", "A", "B", obs_time, Decimal("10")))
        rows.append(_create_row("A", "A", "B", obs_time, Decimal("15")))
        rows.append(_create_row("A", "B", "A", obs_time, Decimal("5")))
        rows.append(_create_row("B", "B", "A", obs_time, Decimal("10")))
        rows.append(_create_row("A", "A", "C", obs_time, Decimal("20")))
        rows.append(_create_row("C", "C", "A", obs_time, Decimal("10")))
        rows.append(_create_row("C", "C", "A", obs_time, Decimal("5")))

    return quarterly_metering_point_time_series_factories.create(spark, rows)


def _create_row(
    domain: str, in_domain: str, out_domain: str, timestamp: datetime, quantity: Decimal
) -> Row:
    return quarterly_metering_point_time_series_factories.create_row(
        grid_area=domain,
        to_grid_area=in_domain,
        from_grid_area=out_domain,
        metering_point_type=MeteringPointType.EXCHANGE,
        quantity=quantity,
        observation_time=timestamp,
    )


def test_aggregate_net_exchange_per_neighbour_ga_single_hour(single_quarter_test_data):
    df = aggregate_net_exchange_per_neighbour_ga(
        single_quarter_test_data, ALL_GRID_AREAS
    ).df.orderBy(Colname.to_grid_area, Colname.from_grid_area, Colname.observation_time)
    values = df.collect()
    assert df.count() == 4
    assert values[0][Colname.to_grid_area] == "A"
    assert values[1][Colname.from_grid_area] == "C"
    assert values[2][Colname.to_grid_area] == "B"
    assert values[0][Colname.quantity] == Decimal("10")
    assert values[1][Colname.quantity] == Decimal("5")
    assert values[2][Colname.quantity] == Decimal("-10")
    assert values[3][Colname.quantity] == Decimal("-5")


def test_aggregate_net_exchange_per_neighbour_ga_multi_hour(multi_quarter_test_data):
    df = aggregate_net_exchange_per_neighbour_ga(
        multi_quarter_test_data, ALL_GRID_AREAS
    ).df.orderBy(Colname.to_grid_area, Colname.from_grid_area, Colname.observation_time)
    values = df.collect()
    assert df.count() == 384
    assert values[0][Colname.to_grid_area] == "A"
    assert values[0][Colname.from_grid_area] == "B"
    assert (
        values[0][Colname.observation_time].strftime(date_time_formatting_string)
        == "2020-01-01T00:00:00"
    )
    assert values[0][Colname.quantity] == Decimal("10")
    assert values[19][Colname.to_grid_area] == "A"
    assert values[19][Colname.from_grid_area] == "B"
    assert (
        values[19][Colname.observation_time].strftime(date_time_formatting_string)
        == "2020-01-01T04:45:00"
    )
    assert values[19][Colname.quantity] == Decimal("10")
