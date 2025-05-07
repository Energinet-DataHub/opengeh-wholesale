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
from datetime import UTC, datetime, timedelta
from unittest.mock import patch

import pytest
from pyspark.sql import DataFrame, SparkSession

import tests.calculation.preparation.transformations.metering_point_periods_factory as factory
from geh_wholesale.calculation.preparation.transformations.grid_loss_metering_point_periods import (
    get_grid_loss_metering_point_periods,
)
from geh_wholesale.codelists import MeteringPointType
from geh_wholesale.constants import Colname
from geh_wholesale.databases import wholesale_internal
from geh_wholesale.databases.wholesale_internal import WholesaleInternalRepository
from geh_wholesale.databases.wholesale_internal.schemas import (
    grid_loss_metering_point_ids_schema,
)
from tests.calculation.energy.grid_loss_metering_point_periods_factories import (
    DEFAULT_FROM_DATE,
    DEFAULT_TO_DATE,
)


@patch.object(wholesale_internal, WholesaleInternalRepository.__name__)
def test__get_grid_loss_metering_point_periods__given_three_metering_point_period_dataframes_on_the_same_grid_area__then_only_return_those_in_the_grid_area_metering_points(
    repository_mock: WholesaleInternalRepository, spark: SparkSession
) -> None:
    # Arrange
    grid_areas = ["804"]
    metering_point_id_1 = "571313180480500149"
    metering_point_id_2 = "571313180400100657"
    metering_point_id_3 = "571313180400100888"
    row1 = factory.create_row(
        metering_point_id=metering_point_id_1,
        grid_area="804",
        metering_point_type=MeteringPointType.PRODUCTION,
    )
    row2 = factory.create_row(
        metering_point_id=metering_point_id_2,
        grid_area="804",
        metering_point_type=MeteringPointType.CONSUMPTION,
    )
    row3 = factory.create_row(
        metering_point_id=metering_point_id_3,
        grid_area="804",
        metering_point_type=MeteringPointType.CONSUMPTION,
    )
    metering_point_periods = factory.create(spark, data=[row1, row2, row3])

    grid_loss_metering_point_ids = spark.createDataFrame(
        [
            (metering_point_id_1,),
            (metering_point_id_2,),
        ],
        grid_loss_metering_point_ids_schema,
    )

    # Act
    repository_mock.read_grid_loss_metering_point_ids.return_value = grid_loss_metering_point_ids
    actual_df = get_grid_loss_metering_point_periods(
        grid_areas,
        metering_point_periods,
        DEFAULT_FROM_DATE,
        DEFAULT_TO_DATE,
        repository_mock,
    )

    # Assert
    actual = actual_df.df.collect()
    assert len(actual) == 2
    assert actual[0][Colname.metering_point_id] == metering_point_id_1
    assert actual[1][Colname.metering_point_id] == metering_point_id_2


@patch.object(wholesale_internal, WholesaleInternalRepository.__name__)
def test__get_grid_loss_metering_point_periods__given_metering_point_period_with_same_id_int_different_observation_time__then_return_expected_amount(
    repository_mock: WholesaleInternalRepository,
    spark: SparkSession,
) -> None:
    # Arrange
    grid_areas = ["804"]
    metering_point_id_1 = "571313180480500149"
    metering_point_id_2 = "571313180400100657"
    row1 = factory.create_row(
        metering_point_id=metering_point_id_1,
        grid_area="804",
        metering_point_type=MeteringPointType.PRODUCTION,
        to_date=None,
    )
    row2 = factory.create_row(
        metering_point_id=metering_point_id_2,
        grid_area="804",
        metering_point_type=MeteringPointType.CONSUMPTION,
        from_date=datetime(2019, 12, 31, 23, 0, tzinfo=UTC),
        to_date=datetime(2020, 1, 1, 23, 0, tzinfo=UTC),
    )
    row3 = factory.create_row(
        metering_point_id=metering_point_id_2,
        grid_area="804",
        metering_point_type=MeteringPointType.CONSUMPTION,
        from_date=datetime(2020, 1, 1, 23, 0, tzinfo=UTC),
        to_date=datetime(2020, 1, 2, 23, 0, tzinfo=UTC),
    )
    metering_point_period = factory.create(spark, data=[row1, row2, row3])

    grid_loss_metering_points = spark.createDataFrame(
        [
            (metering_point_id_1,),
            (metering_point_id_2,),
        ],
        grid_loss_metering_point_ids_schema,
    )

    # Act
    repository_mock.read_grid_loss_metering_point_ids.return_value = grid_loss_metering_points
    grid_loss_metering_point_periods = get_grid_loss_metering_point_periods(
        grid_areas,
        metering_point_period,
        DEFAULT_FROM_DATE,
        DEFAULT_TO_DATE + timedelta(days=1),
        repository_mock,
    )

    # Assert
    assert grid_loss_metering_point_periods.df.count() == 3


@pytest.mark.acceptance_test
@patch.object(wholesale_internal, WholesaleInternalRepository.__name__)
def test__get_grid_loss_metering_point_periods__when_no_grid_loss_metering_point_id_in_grid_area__raise_exception(
    repository_mock: WholesaleInternalRepository, spark: SparkSession
) -> None:
    # Arrange
    grid_area = "grid_area_without_grid_loss_metering_point"
    metering_point_periods = factory.create(spark, data=[])
    grid_loss_metering_point_ids = spark.createDataFrame([], grid_loss_metering_point_ids_schema)
    repository_mock.read_grid_loss_metering_point_ids.return_value = grid_loss_metering_point_ids

    # Act and Assert
    with pytest.raises(
        ValueError,
        match=f"The following grid areas are missing positive or negative grid loss metering points: {grid_area}",
    ):
        get_grid_loss_metering_point_periods(
            [grid_area],
            metering_point_periods,
            DEFAULT_FROM_DATE,
            DEFAULT_TO_DATE,
            repository_mock,
        )


CALCULATION_PERIOD_START = datetime(2020, 1, 1, 0, 0, tzinfo=UTC)
GRID_AREA = "804"
POSITIVE_GRID_LOSS_METERING_POINT_ID = "571313180480500149"
NEGATIVE_GRID_LOSS_METERING_POINT_ID = "571313180400100657"


def _create_case(spark: SparkSession, offsets: list[tuple[int, int | None]]) -> DataFrame:
    metering_point_periods = []

    # Create positive grid loss metering points
    for offset in offsets:
        positive_grid_loss_metering_point = factory.create_row(
            metering_point_id=POSITIVE_GRID_LOSS_METERING_POINT_ID,
            grid_area=GRID_AREA,
            metering_point_type=MeteringPointType.CONSUMPTION,
            from_date=CALCULATION_PERIOD_START + timedelta(days=offset[0]),
            to_date=CALCULATION_PERIOD_START + timedelta(days=offset[1]) if offset[1] else None,
        )
        metering_point_periods.append(positive_grid_loss_metering_point)

    # Create negative grid loss metering points spanning the whole calculation period
    factory.create_row(
        metering_point_id=NEGATIVE_GRID_LOSS_METERING_POINT_ID,
        grid_area=GRID_AREA,
        metering_point_type=MeteringPointType.PRODUCTION,
        from_date=CALCULATION_PERIOD_START,
        to_date=None,
    )
    metering_point_periods.append(positive_grid_loss_metering_point)

    return factory.create(spark, data=metering_point_periods)


@pytest.mark.parametrize(
    "metering_point_periods_offsets",
    [
        # Day offsets:                0    4                31
        # Case: From day 4 the positive grid loss metering point is missing
        # Calculation period:         +---------------------+
        # Metering point periods:     +----+
        [(0, 4)],
        # Case: Day 0 is missing the positive grid loss metering point
        # Calculation period:         +---------------------+
        # Metering point periods:      +----+--------------------
        [(1, 4), (4, None)],
        # Case: Day 4 is missing the positive grid loss metering point
        # Calculation period:         +---------------------+
        # Metering point periods:   +-----+ +-------------------
        [(-1, 4), (5, None)],
    ],
)
def test__get_grid_loss_metering_point_periods__when_missing_grid_loss_metering_point__raises_exception(
    spark: SparkSession,
    monkeypatch: pytest.MonkeyPatch,
    metering_point_periods_offsets: list[tuple[int, int | None]],
) -> None:
    """The test always creates a negative grid loss metering point that spans the whole calculation period.
    So the testing of failure is only dependent on the positive grid loss metering point.
    """
    # Arrange: Create metering point periods
    metering_point_periods = _create_case(spark, metering_point_periods_offsets)

    # Arrange: Create grid loss metering points
    grid_loss_metering_point_ids = spark.createDataFrame(
        [(POSITIVE_GRID_LOSS_METERING_POINT_ID,), (NEGATIVE_GRID_LOSS_METERING_POINT_ID,)],
        grid_loss_metering_point_ids_schema,
    )

    # Arrange: Mock the repository to return the grid loss metering points
    repository = wholesale_internal.WholesaleInternalRepository(spark, "catalog-name", "table-name")
    monkeypatch.setattr(
        repository,
        WholesaleInternalRepository.read_grid_loss_metering_point_ids.__name__,
        lambda: grid_loss_metering_point_ids,
    )

    # Act and Assert
    with pytest.raises(
        ValueError,
        match=f"The following grid areas are missing positive or negative grid loss metering points: {GRID_AREA}",
    ):
        get_grid_loss_metering_point_periods(
            [GRID_AREA],
            metering_point_periods,
            CALCULATION_PERIOD_START,
            CALCULATION_PERIOD_START + timedelta(days=31),
            repository,
        )
