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


class StorageColumnNames:
    balance_responsible_id = "balance_responsible_id"
    """Obsolete. Use balance_responsible_party_id"""
    balance_responsible_party_id = "balance_responsible_id"
    charge_code = "charge_code"
    charge_key = "charge_key"
    charge_owner_id = "charge_owner_id"
    charge_price = "charge_price"
    charge_time = "charge_time"
    charge_type = "charge_type"
    calculation_execution_time_start = "calculation_execution_time_start"
    calculation_id = "calculation_id"
    calculation_result_id = "calculation_result_id"
    calculation_type = "calculation_type"
    energy_supplier_id = "energy_supplier_id"
    from_date = "from_date"
    from_grid_area_code = "from_grid_area_code"
    grid_area_code = "grid_area_code"
    is_tax = "is_tax"
    metering_point_id = "metering_point_id"
    metering_point_type = "metering_point_type"
    neighbor_grid_area_code = "neighbor_grid_area_code"
    observation_time = "observation_time"
    parent_metering_point_id = "parent_metering_point_id"
    quantity = "quantity"
    quality = "quality"
    quantity_qualities = "quantity_qualities"
    resolution = "resolution"
    settlement_method = "settlement_method"
    time = "time"
    time_series_type = "time_series_type"
    to_date = "to_date"
    to_grid_area_code = "to_grid_area_code"
