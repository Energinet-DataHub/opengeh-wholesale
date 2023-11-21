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

from azure.identity import ClientSecretCredential
from dataclasses import dataclass
from datetime import datetime
from package.codelists.process_type import ProcessType


@dataclass
class CalculatorArgs:
    data_storage_account_name: str
    data_storage_account_credentials: ClientSecretCredential
    wholesale_container_path: str
    calculation_input_path: str
    batch_id: str
    batch_grid_areas: list[str]
    batch_period_start_datetime: datetime
    batch_period_end_datetime: datetime
    batch_process_type: ProcessType
    batch_execution_time_start: datetime
    time_zone: str
