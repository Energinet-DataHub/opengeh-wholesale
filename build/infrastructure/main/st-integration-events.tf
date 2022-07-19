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

resource "azurerm_storage_container" "integration_events_container" {
  name                  = local.INTERGRATION_EVENTS_CONTAINER_NAME
  storage_account_name  = data.azurerm_key_vault_secret.st_shared_data_lake_name.value
}

resource "azurerm_storage_container" "market_participant_events_container" {
  name                  = local.MARKET_PARTICIPANT_EVENTS_CONTAINER_NAME
  storage_account_name  = data.azurerm_key_vault_secret.st_shared_data_lake_name.value
}