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


# TODO BJM: Remove when only using Unity Catalog
class AggregationLevel(Enum):
    GRID_AREA = "total_ga"  # rename to "grid_area"
    BALANCE_RESPONSIBLE_PARTY = "brp_ga"  # rename to "balance_responsible_party"
    ENERGY_SUPPLIER = "es_brp_ga"  # rename to "energy_supplier"
