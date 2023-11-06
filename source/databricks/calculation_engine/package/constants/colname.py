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
    positive_grid_loss = "positive_grid_loss"
    negative_grid_loss = "negative_grid_loss"
    aggregation_level = "aggregation_level"
    balance_responsible_id = "balance_responsible_id"
    batch_execution_time_start = "batch_execution_time_start"
    batch_id = "batch_id"
    batch_process_type = "batch_process_type"
    calculation_type = "calculation_type"
    charge_count = "charge_count"
    charge_code = "charge_code"
    charge_key = "charge_key"
    charge_owner = "charge_owner_id"
    charge_price = "charge_price"
    charge_tax = "is_tax"
    charge_time = "charge_time"
    charge_type = "charge_type"
    currency = "currency"
    date = "date"
    end = "end"
    energy_supplier_id = "energy_supplier_id"
    from_date = "from_date"
    from_grid_area = "from_grid_area_code"
    "The grid area sending current"
    grid_area = "grid_area_code"
    is_positive_grid_loss_responsible = "is_positive_grid_loss_responsible"
    is_negative_grid_loss_responsible = "is_negative_grid_loss_responsible"
    local_date = "localDate"
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
    start = "start"
    start_datetime = "start_datetime"
    sum_quantity = "sum_quantity"
    time_series_type = "time_series_type"
    time_window = "time_window"
    time_window_end = "time_window.end"
    """The end element of the time_window struct type column"""
    time_window_start = "time_window.start"
    """The start element of the time_window struct type column"""
    to_date = "to_date"
    to_grid_area = "to_grid_area_code"
    "The grid area receiving current"
    total_daily_charge_price = "total_daily_charge_price"
    total_amount = "total_amount"
    total_quantity = "total_quantity"
    unit = "unit"
