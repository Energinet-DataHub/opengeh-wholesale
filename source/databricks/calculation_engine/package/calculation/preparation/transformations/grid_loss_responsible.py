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
"""
By having a conftest.py in this directory, we are able to add all packages
defined in the geh_stream directory in our tests.
"""

from pyspark.sql import DataFrame
from pyspark.sql.functions import col

from package.calculation.preparation.grid_loss_responsible import (
    GridLossResponsible,
)
from package.codelists import MeteringPointType
from package.constants import Colname
from package.calculation_input import TableReader


def get_grid_loss_responsible(
    grid_areas: list[str],
    metering_point_periods_df: DataFrame,
    table_reader: TableReader,
) -> GridLossResponsible:
    grid_loss_responsible = (
        table_reader.read_grid_loss_metering_points()
        .join(
            metering_point_periods_df,
            Colname.metering_point_id,
            "inner",
        )
        .select(
            col(Colname.metering_point_id),
            col(Colname.grid_area),
            col(Colname.from_date),
            col(Colname.to_date),
            col(Colname.metering_point_type),
            col(Colname.energy_supplier_id),
        )
    )

    _throw_if_no_grid_loss_responsible(grid_areas, grid_loss_responsible)

    return GridLossResponsible(grid_loss_responsible)


def _throw_if_no_grid_loss_responsible(
    grid_areas: list[str], grid_loss_responsible_df: DataFrame
) -> None:
    for grid_area in grid_areas:
        current_grid_loss_metering_points = grid_loss_responsible_df.filter(
            col(Colname.grid_area) == grid_area
        )
        if (
            current_grid_loss_metering_points.filter(
                col(Colname.metering_point_type) == MeteringPointType.PRODUCTION.value
            ).count()
            == 0
        ):
            raise ValueError(
                f"No responsible for negative grid loss found for grid area {grid_area}"
            )
        if (
            current_grid_loss_metering_points.filter(
                col(Colname.metering_point_type) == MeteringPointType.CONSUMPTION.value
            ).count()
            == 0
        ):
            raise ValueError(
                f"No responsible for positive grid loss found for grid area {grid_area}"
            )
