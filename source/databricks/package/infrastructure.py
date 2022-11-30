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

# Resource names and variables defined in the infrastructure repository (https://github.com/Energinet-DataHub/dh3-infrastructure)

WHOLESALE_CONTAINER_NAME = "wholesale"
PROCESSES_CONTAINER_NAME = "processes"
INTEGRATION_EVENTS_CONTAINER_NAME = "integration-events"

RESULTS_FOLDER_NAME = "results"
EVENTS_FOLDER_NAME = "events"
EVENTS_CHECKPOINT_FOLDER_NAME = "events-checkpoint"


def get_storage_account_url(storage_account_name: str) -> str:
    return f"https://{storage_account_name}.dfs.core.windows.net"


def get_integration_events_path(storage_account_name: str) -> str:
    return f"abfss://{INTEGRATION_EVENTS_CONTAINER_NAME}@{storage_account_name}.dfs.core.windows.net/{EVENTS_FOLDER_NAME}"


def get_integration_events_checkpoint_path(storage_account_name: str) -> str:
    return f"abfss://{INTEGRATION_EVENTS_CONTAINER_NAME}@{storage_account_name}.dfs.core.windows.net/{EVENTS_CHECKPOINT_FOLDER_NAME}"


def get_process_results_path(storage_account_name: str) -> str:
    return f"abfss://{PROCESSES_CONTAINER_NAME}@{storage_account_name}.dfs.core.windows.net/{RESULTS_FOLDER_NAME}"
