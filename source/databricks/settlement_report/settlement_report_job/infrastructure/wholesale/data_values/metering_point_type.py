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


class MeteringPointTypeDataProductValue(Enum):
    PRODUCTION = "production"
    CONSUMPTION = "consumption"
    EXCHANGE = "exchange"
    # The following are for child metering points
    VE_PRODUCTION = "ve_production"
    NET_PRODUCTION = "net_production"
    SUPPLY_TO_GRID = "supply_to_grid"
    CONSUMPTION_FROM_GRID = "consumption_from_grid"
    WHOLESALE_SERVICES_INFORMATION = "wholesale_services_information"
    OWN_PRODUCTION = "own_production"
    NET_FROM_GRID = "net_from_grid"
    NET_TO_GRID = "net_to_grid"
    TOTAL_CONSUMPTION = "total_consumption"
    ELECTRICAL_HEATING = "electrical_heating"
    NET_CONSUMPTION = "net_consumption"
    capacity_settlement = "capacity_settlement"
