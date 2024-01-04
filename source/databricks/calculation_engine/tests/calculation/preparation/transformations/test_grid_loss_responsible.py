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
from unittest.mock import patch

from package.calculation.preparation.transformations.grid_loss_responsible import (
    get_grid_loss_responsible,
    grid_area_responsible_schema,
)
import metering_point_periods_factory as factory
from package.codelists import MeteringPointType
from package.constants import Colname


def test__get_grid_loss_responsible__given_three_metering_point_period_dataframes_on_the_same_grid_area__then_only_return_the_once_in_the_grid_area_responsible_list(
    spark: SparkSession,
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
    metering_point_period = factory.create(spark, data=[row1, row2, row3])

    grid_area_responsible = spark.createDataFrame(
        [
            (metering_point_id_1,),
            (metering_point_id_2,),
        ],
        grid_area_responsible_schema,
    )

    # Act
    with patch(
        "package.calculation.preparation.transformations.grid_loss_responsible._get_all_grid_loss_responsible",
        return_value=grid_area_responsible,
    ):
        grid_loss_responsible = get_grid_loss_responsible(
            grid_areas, metering_point_period
        )

    # Assert
    assert grid_loss_responsible.df.count() == 2
    assert (
        grid_loss_responsible.df.collect()[0][Colname.metering_point_id]
        == metering_point_id_1
    )
    assert (
        grid_loss_responsible.df.collect()[1][Colname.metering_point_id]
        == metering_point_id_2
    )


@pytest.mark.acceptance_test
def test__get_grid_loss_responsible__when_no_grid_loss_responsible_in_grid_area__raise_exception() -> (
    None
):
    # Arrange
    grid_areas = ["grid_area_without_grid_loss_responsible"]

    # Act and Assert
    with pytest.raises(Exception):
        get_grid_loss_responsible(grid_areas)
