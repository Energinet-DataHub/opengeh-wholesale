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


class InputMeteringPointType(Enum):
    """This type should be replaced by `MeteringPointType` when the contract with the migration domain has been updated"""

    PRODUCTION = "E18"
    CONSUMPTION = "E17"
    EXCHANGE = "E20"
    VE_PRODUCTION = "D01"
    NET_PRODUCTION = "D05"
    SUPPLY_TO_GRID = "D06"
    CONSUMPTION_FROM_GRID = "D07"
    WHOLESALE_SERVICES_INFORMATION = "D08"
    OWN_PRODUCTION = "D09"
    NET_FROM_GRID = "D10"
    NET_TO_GRID = "D11"
    TOTAL_CONSUMPTION = "D12"
    ELECTRICAL_HEATING = "D14"
    NET_CONSUMPTION = "D15"
    EFFECT_SETTLEMENT = "D19"
