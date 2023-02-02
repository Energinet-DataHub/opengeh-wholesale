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
    market_role = "market_role"
    added_grid_loss = "added_grid_loss"
    added_system_correction = "added_system_correction"
    aggregated_quality = "aggregated_quality"
    balance_responsible_id = "BalanceResponsibleId"
    charge_count = "charge_count"
    charge_id = "charge_id"
    charge_key = "charge_key"
    charge_owner = "charge_owner"
    charge_price = "charge_price"
    charge_tax = "charge_tax"
    charge_time = "time"
    charge_type = "charge_type"
    currency = "currency"
    date = "date"
    day = "Day"
    domain = "domain"
    end = "end"
    end_datetime = "end_datetime"
    energy_supplier_id = "EnergySupplierId"
    event_id = "event_id"
    event_name = "event_name"
    from_date = "FromDate"
    gln = "gln"
    grid_area = "GridAreaCode"
    grid_loss = "grid_loss"
    gsrn_number = "gsrn_number"
    in_grid_area = "FromGridAreaCode"
    is_grid_loss = "is_grid_loss"
    is_system_correction = "is_system_correction"
    job_id = "job_id"
    metering_method = "metering_method"
    metering_point_id = "MeteringPointId"
    metering_point_type = "Type"
    month = "Month"
    net_settlement_group = "net_settlement_group"
    observation_time = "ObservationTime"
    """When the production/consumption/exchange actually happened."""
    out_grid_area = "ToGridAreaCode"
    parent_metering_point_id = "parent_metering_point_id"
    position = "position"
    price_per_day = "price_per_day"
    processed_date = "processed_date"
    product = "product"
    qualities = "qualities"
    "The set of distinct qualities in the current time window for the current grouping"
    quality = "Quality"
    quantity = "Quantity"
    resolution = "Resolution"
    result_id = "result_id"
    result_name = "result_name"
    result_path = "result_path"
    settlement_method = "SettlementMethod"
    start = "start"
    start_datetime = "start_datetime"
    sum_quantity = "sum_quantity"
    time_series_type = "time_series_type"
    time_window = "time_window"
    time_window_end = "time_window.end"
    time_window_start = "time_window.start"
    to_date = "ToDate"
    total_daily_charge_price = "total_daily_charge_price"
    total_amount = "total_amount"
    total_quantity = "total_quantity"
    unit = "unit"
    year = "Year"
