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

from pyspark.sql import DataFrame, SparkSession, Row

from package.constants import Colname


def get_batch_grid_areas_df(
    batch_grid_areas: list[str], spark: SparkSession
) -> DataFrame:
    return spark.createDataFrame(
        map(lambda x: Row(str(x)), batch_grid_areas), [Colname.grid_area]
    )


def check_all_grid_areas_have_metering_points(
    batch_grid_areas_df: DataFrame, master_basis_data_df: DataFrame
) -> None:
    """Raises exception if any grid area has no metering points"""

    distinct_grid_areas_rows_df = master_basis_data_df.select(
        Colname.grid_area
    ).distinct()
    grid_area_with_no_metering_point_df = batch_grid_areas_df.join(
        distinct_grid_areas_rows_df, Colname.grid_area, "leftanti"
    )

    if grid_area_with_no_metering_point_df.count() > 0:
        grid_areas_to_inform_about = grid_area_with_no_metering_point_df.select(
            Colname.grid_area
        ).collect()

        grid_area_codes_to_inform_about = map(
            lambda x: x.__getitem__(Colname.grid_area), grid_areas_to_inform_about
        )
        raise Exception(
            f"There are no metering points for the grid areas {list(grid_area_codes_to_inform_about)} in the requested period"
        )
