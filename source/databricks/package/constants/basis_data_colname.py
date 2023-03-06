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


class BasisDataColname:
    energy_supplier_id = "ENERGYSUPPLIERID"
    grid_area = "GRIDAREA"
    in_grid_area = "FROMGRIDAREA"
    metering_point_id = "METERINGPOINTID"
    metering_point_type = "TYPEOFMP"
    out_grid_area = "TOGRIDAREA"
    quantity_prefix = "ENERGYQUANTITY"
    "The column name prefix. The full quantity column names are suffixed with a number. E.g. 'ENERGYQUANTITY1'"
    settlement_method = "SETTLEMENTMETHOD"
    start_datetime = "STARTDATETIME"
    valid_from = "VALIDFROM"
    valid_to = "VALIDTO"
