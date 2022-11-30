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

from dataclasses import dataclass
from .args_helper import valid_date, valid_list


@dataclass
class CalculatorArgs:
    data_storage_account_name: str
    data_storage_account_key: str
    integration_events_path: str
    time_series_points_path: str
    process_results_path: str
    batch_id: str
    batch_grid_areas: valid_list
    batch_snapshot_datetime: valid_date
    batch_period_start_datetime: valid_date
    batch_period_end_datetime: valid_date
    time_zone: str
