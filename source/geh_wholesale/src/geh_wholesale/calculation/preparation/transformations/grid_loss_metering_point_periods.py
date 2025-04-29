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
"""By having a conftest.py in this directory, we are able to add all packages defined in the geh_stream directory in our tests."""

from datetime import datetime

from pyspark.sql import DataFrame
from pyspark.sql.functions import col

from geh_wholesale.calculation.preparation.data_structures.grid_loss_metering_point_periods import (
    GridLossMeteringPointPeriods,
)
from geh_wholesale.codelists import MeteringPointType
from geh_wholesale.constants import Colname
from geh_wholesale.databases import wholesale_internal


def get_grid_loss_metering_point_periods(
    grid_areas: list[str],
    metering_point_periods_df: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
    repository: wholesale_internal.WholesaleInternalRepository,
) -> GridLossMeteringPointPeriods:
    grid_loss_metering_point_periods = (
        repository.read_grid_loss_metering_point_ids()
        .join(
            metering_point_periods_df,
            Colname.metering_point_id,
            "inner",
        )
        .select(
            col(Colname.metering_point_id),
            col(Colname.grid_area_code),
            col(Colname.from_date),
            col(Colname.to_date),
            col(Colname.metering_point_type),
            col(Colname.energy_supplier_id),
            col(Colname.balance_responsible_party_id),
        )
        .where(
            (col(Colname.from_date) <= period_start_datetime)
            & (col(Colname.to_date).isNull() | (col(Colname.to_date) >= period_end_datetime))
        )
    )

    _throw_if_no_grid_loss_metering_point_periods_in_grid_area(grid_areas, grid_loss_metering_point_periods)

    return GridLossMeteringPointPeriods(grid_loss_metering_point_periods)


def _throw_if_no_grid_loss_metering_point_periods_in_grid_area(
    grid_areas: list[str], grid_loss_metering_point_periods: DataFrame
) -> None:
    for grid_area in grid_areas:
        current_grid_loss_metering_point_periods = grid_loss_metering_point_periods.where(
            col(Colname.grid_area_code) == grid_area
        )
        if (
            current_grid_loss_metering_point_periods.filter(
                col(Colname.metering_point_type) == MeteringPointType.PRODUCTION.value
            ).count()
            == 0
        ):
            raise ValueError(f"No metering point for negative grid loss found for grid area {grid_area}")
        if (
            current_grid_loss_metering_point_periods.filter(
                col(Colname.metering_point_type) == MeteringPointType.CONSUMPTION.value
            ).count()
            == 0
        ):
            raise ValueError(f"No metering point for positive grid loss found for grid area {grid_area}")
