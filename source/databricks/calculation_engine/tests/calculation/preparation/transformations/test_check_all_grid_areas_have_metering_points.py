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
from package.calculation.preparation.transformations.batch_grid_areas import (
    check_all_grid_areas_have_metering_points,
)
from package.codelists import (
    MeteringPointResolution,
)
from package.constants import Colname


@pytest.fixture(scope="module")
def metering_point_period_df_factory(spark, timestamp_factory):
    def factory(
        grid_area_code,
    ):
        df = [
            {
                Colname.metering_point_id: "a-metering-point-id",
                Colname.grid_area: grid_area_code,
                Colname.metering_point_type: "the_metering_point_type",
                Colname.from_date: timestamp_factory("2022-01-01T22:00:00.000Z"),
                Colname.to_date: timestamp_factory("2022-01-11T22:00:00.000Z"),
                Colname.resolution: MeteringPointResolution.HOUR.value,
            }
        ]
        return spark.createDataFrame(df)

    return factory


@pytest.fixture
def grid_area_df_factory(spark):
    def factory(grid_area_code="805"):
        row = {
            Colname.grid_area: grid_area_code,
            "GridAreaLinkId": "grid_area_link_id",
        }
        return spark.createDataFrame([row])

    return factory


def test__when_all_grid_areas_have_metering_points_does_not_throw_exception(
    grid_area_df_factory,
    metering_point_period_df_factory,
):
    # Arrange
    grid_area_df = grid_area_df_factory(grid_area_code="805")

    # Act
    metering_points_df = metering_point_period_df_factory(grid_area_code="805")

    # Assert
    check_all_grid_areas_have_metering_points(
        grid_area_df,
        metering_points_df,
    )


def test__when_no_metering_point_exist_in_grid_area_exception_is_thrown(
    grid_area_df_factory,
    metering_point_period_df_factory,
):
    # Arrange
    grid_area_df = grid_area_df_factory(grid_area_code="805").union(
        grid_area_df_factory(grid_area_code="806")
    )

    # Act
    metering_points_df = metering_point_period_df_factory(grid_area_code="806")

    with pytest.raises(Exception) as e_info:
        check_all_grid_areas_have_metering_points(
            grid_area_df,
            metering_points_df,
        )
    # assert
    assert (
        str(e_info.value)
        == "There are no metering points for the grid areas ['805'] in the requested period"
    )
