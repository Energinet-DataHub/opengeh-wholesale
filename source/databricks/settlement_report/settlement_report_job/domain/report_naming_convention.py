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

METERING_POINT_TYPES = {
    "ve_production": "D01",
    "net_production": "D05",
    "supply_to_grid": "D06",
    "consumption_from_grid": "D07",
    "wholesale_services_information": "D08",
    "own_production": "D09",
    "net_from_grid": "D10",
    "net_to_grid": "D11",
    "total_consumption": "D12",
    "electrical_heating": "D14",
    "net_consumption": "D15",
    "effect_settlement": "D19",
    "consumption": "E17",
    "production": "E18",
    "exchange": "E20",
}

SETTLEMENT_METHODS = {
    "non_profiled": "E02",
    "flex": "D01",
}

CALCULATION_TYPES_TO_ENERGY_BUSINESS_PROCESS = {
    "balance_fixing": "D04",
    "wholesale_fixing": "D05",
    "first_correction_settlement": "D32",
    "second_correction_settlement": "D32",
    "third_correction_settlement": "D32",
}

CALCULATION_TYPES_TO_PROCESS_VARIANT = {
    "first_correction_settlement": "1ST",
    "second_correction_settlement": "2ND",
    "third_correction_settlement": "3RD",
}

RESOLUTION_NAMES = {"PT1H": "TSSD60", "PT15M": "TSSD15"}
