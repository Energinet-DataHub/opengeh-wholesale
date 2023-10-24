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

from enum import Enum


class TimeSeriesType(Enum):
    """
    The type of the aggregated time series type of an energy calculation.
    """

    PRODUCTION = "production"
    NON_PROFILED_CONSUMPTION = "non_profiled_consumption"
    NET_EXCHANGE_PER_NEIGHBORING_GA = "net_exchange_per_neighboring_ga"
    NET_EXCHANGE_PER_GA = "net_exchange_per_ga"
    FLEX_CONSUMPTION = "flex_consumption"
    GRID_LOSS = "grid_loss"
    NEGATIVE_GRID_LOSS = "negative_grid_loss"
    POSITIVE_GRID_LOSS = "positive_grid_loss"
    TOTAL_CONSUMPTION = "total_consumption"
    TEMP_FLEX_CONSUMPTION = "temp_flex_consumption"
    TEMP_PRODUCTION = "temp_production"
