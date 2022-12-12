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


class ResultKeyName():
    aggregation_base_dataframe = 0
    grid_loss_sys_cor_master_data = 1
    net_exchange_per_neighbour = 10
    net_exchange_per_ga = 20
    hourly_consumption = 30
    flex_consumption = 40
    hourly_production = 50
    grid_loss = 60
    added_system_correction = 70
    added_grid_loss = 80
    combined_system_correction = 90
    combined_grid_loss = 100
    flex_consumption_with_grid_loss = 110
    hourly_production_with_system_correction_and_grid_loss = 120
    hourly_production_ga_es = 130
    hourly_settled_consumption_ga_es = 140
    flex_settled_consumption_ga_es = 150
    hourly_production_ga_brp = 160
    hourly_settled_consumption_ga_brp = 170
    flex_settled_consumption_ga_brp = 180
    hourly_production_ga = 190
    hourly_settled_consumption_ga = 200
    flex_settled_consumption_ga = 210
    total_consumption = 220
    residual_ga = 230
    hourly_tariffs = "hourly_tariffs"
    daily_tariffs = "daily_tariffs"
    subscription_prices = "subscription_prices"
    fee_prices = "fee_prices"
