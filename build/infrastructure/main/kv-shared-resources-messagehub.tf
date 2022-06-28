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

data "azurerm_key_vault_secret" "messagehub_service_bus_connection_string" {
  name         = "todo"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "messagehub_data_available_queue_name" {
  name         = "sbq-data-available-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "messagehub_reply_queue_name" {
  name         = "sbq-wholesale-reply-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "messagehub_storage_connection_string" {
  name         = "todo"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "messagehub_storage_container_name" {
  name         = "st-marketres-postofficereply-container-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

# ????????????????????
data "azurerm_key_vault_secret" "messagehub_request_queue" {
  name         = "sbq-wholesale-dequeue-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
