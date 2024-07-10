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

from package.calculation.energy.aggregators.transformations.aggregate_sum_and_quality import (
    aggregate_sum_quantity_and_qualities,
)
from package.calculation.energy.data_structures.energy_results import EnergyResults
from package.constants import Colname


def aggregate(df: EnergyResults) -> EnergyResults:
    group_by = [Colname.grid_area_code, Colname.observation_time]
    result = aggregate_sum_quantity_and_qualities(df.df, group_by)
    return EnergyResults(result)


def aggregate_per_brp(df: EnergyResults) -> EnergyResults:
    """Function to aggregate sum per grid area and balance responsible party."""
    group_by = [
        Colname.grid_area_code,
        Colname.balance_responsible_id,
        Colname.observation_time,
    ]
    result = aggregate_sum_quantity_and_qualities(df.df, group_by)
    return EnergyResults(result)
