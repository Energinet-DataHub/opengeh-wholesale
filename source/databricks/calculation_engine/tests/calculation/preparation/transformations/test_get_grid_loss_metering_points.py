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
from datetime import datetime
import pytest
from pyspark.sql import SparkSession, DataFrame, Row
import metering_point_periods_factory as factory
from package.calculation.preparation.data_structures.grid_loss_responsible import (
    GridLossResponsible,
)
from package.calculation.input.schemas.grid_loss_metering_points_schema import (
    grid_loss_metering_points_schema,
)
from package.calculation.preparation.data_structures.grid_loss_responsible import (
    grid_loss_responsible_schema,
)
from package.calculation.preparation.transformations.grid_loss_metering_points import (
    get_grid_loss_metering_points,
)
from package.constants import Colname
from package.codelists import MeteringPointType

from typing import List, Dict


class DefaultValues:
    DEFAULT_GRID_AREA = ["804"]
    DEFAULT_FROM_DATE = datetime(2020, 1, 1, 0, 0)
    DEFAULT_TO_DATE = datetime(2020, 1, 2, 0, 0)
    DEFAULT_METERING_POINT_TYPE = MeteringPointType.PRODUCTION
    DEFAULT_ENERGY_SUPPLIER_ID = "test"


def _create_grid_loss_responsible_data(data: List[str]) -> List[Row]:
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


def _get_grid_loss_responsible_dataframe(
    spark: SparkSession, data: List[str]
) -> GridLossResponsible:
    grid_loss_responsible_list = _create_grid_loss_responsible_data(data)

    metering_point_period = factory.create(spark, data=grid_loss_responsible_list)

    return GridLossResponsible(metering_point_period)


def _get_metering_point_dataframe(spark: SparkSession, data: List[str]) -> DataFrame:
    return spark.createDataFrame(data, grid_loss_metering_points_schema)


@pytest.mark.parametrize(
    "grid_loss_metering_points, expected_count",
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
def test__get_grid_loss_metering_points__count_is_correct(
    spark: SparkSession,
    grid_loss_metering_points: List[str],
    expected_count: int,
):
    # Arrange
    grid_loss_responsible = _get_grid_loss_responsible_dataframe(
        spark, grid_loss_metering_points
    )

    # Act
    result = get_grid_loss_metering_points(grid_loss_responsible)

    # Assert
    assert result.df.count() == expected_count
