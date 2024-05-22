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


class Colname:
    balance_responsible_id = "balance_responsible_id"
    calculation_execution_time_start = "calculation_execution_time_start"
    calculation_id = "calculation_id"
    calculation_type = "calculation_type"
    charge_count = "charge_count"
    charge_code = "charge_code"
    charge_key = "charge_key"
    charge_owner = "charge_owner_id"
    charge_price = "charge_price"
    charge_tax = "is_tax"
    charge_time = "charge_time"
    charge_type = "charge_type"
    created_by_user_id = "created_by_user_id"
    """The user id of the user who created/started the calculation."""
    currency = "currency"
    date = "date"
    energy_supplier_id = "energy_supplier_id"
    from_date = "from_date"
    from_grid_area = "from_grid_area_code"
    "The grid area sending current"
    grid_area = "grid_area_code"
    metering_point_id = "metering_point_id"
    metering_point_type = "type"
    observation_time = "observation_time"
    """When the production/consumption/exchange actually happened."""
    parent_metering_point_id = "parent_metering_point_id"
    price_per_day = "price_per_day"
    qualities = "qualities"
    """Aggregated qualities: An array of unique quality values aggregated from metering point time series."""
    quality = "quality"
    quantity = "quantity"
    resolution = "resolution"
    settlement_method = "settlement_method"
    start_date_time = "start_date_time"
    time_series_type = "time_series_type"
    to_date = "to_date"
    to_grid_area = "to_grid_area_code"
    "The grid area receiving current"
    total_daily_charge_price = "total_daily_charge_price"
    total_amount = "total_amount"
    total_quantity = "total_quantity"
    unit = "unit"
