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

from pyspark.sql import DataFrame
from package.constants import Colname
from package.calculation.preparation.data_structures.grid_loss_responsible import (
    GridLossResponsible,
)
from package.calculation.preparation.data_structures.grid_loss_metering_points import (
    GridLossMeteringPoints,
)


def get_grid_loss_metering_points(
    grid_loss_responsible_df: GridLossResponsible
) -> GridLossMeteringPoints:
    return GridLossMeteringPoints(
        grid_loss_responsible_df
        .df
        .select(Colname.metering_point_id)
        .distinct()
    )
