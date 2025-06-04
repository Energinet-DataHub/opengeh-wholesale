from datetime import datetime
from typing import List

import pytest
from pyspark.sql import Row, SparkSession

import tests.calculation.preparation.transformations.metering_point_periods_factory as factory
from geh_wholesale.calculation.preparation.data_structures.grid_loss_metering_point_periods import (
    GridLossMeteringPointPeriods,
)
from geh_wholesale.calculation.preparation.transformations.grid_loss_metering_point_ids import (
    get_grid_loss_metering_point_ids,
)
from geh_wholesale.codelists import MeteringPointType


class DefaultValues:
    DEFAULT_GRID_AREA = "804"
    DEFAULT_FROM_DATE = datetime(2020, 1, 1, 0, 0)
    DEFAULT_TO_DATE = datetime(2020, 1, 2, 0, 0)
    DEFAULT_METERING_POINT_TYPE = MeteringPointType.PRODUCTION
    DEFAULT_ENERGY_SUPPLIER_ID = "test"


def _create_grid_loss_metering_point_periods(data: List[str]) -> List[Row]:
    resulting_data_frame = []
    for entry in data:
        resulting_data_frame.append(
            factory.create_row(
                metering_point_id=entry,
                grid_area=DefaultValues.DEFAULT_GRID_AREA,
                metering_point_type=DefaultValues.DEFAULT_METERING_POINT_TYPE,
            )
        )
    return resulting_data_frame


def _get_grid_loss_metering_point_periods(spark: SparkSession, data: List[str]) -> GridLossMeteringPointPeriods:
    grid_loss_metering_point_periods = _create_grid_loss_metering_point_periods(data)

    metering_point_period = factory.create(spark, data=grid_loss_metering_point_periods)

    return GridLossMeteringPointPeriods(metering_point_period)


@pytest.mark.parametrize(
    ("grid_loss_metering_points", "expected_count"),
    [
        (
            [
                "A",
                "B",
                "C",
                "D",
                "E",
                "F",
            ],
            6,
        ),
        (
            [
                "A",
                "B",
                "C",
                "C",
                "D",
                "E",
                "F",
            ],
            6,
        ),
        ([], 0),
    ],
)
def test__get_grid_loss_metering_point_ids__count_is_correct(
    spark: SparkSession,
    grid_loss_metering_points: list[str],
    expected_count: int,
) -> None:
    # Arrange
    grid_loss_metering_point_periods = _get_grid_loss_metering_point_periods(spark, grid_loss_metering_points)

    # Act
    result = get_grid_loss_metering_point_ids(grid_loss_metering_point_periods)

    # Assert
    assert result.df.count() == expected_count
