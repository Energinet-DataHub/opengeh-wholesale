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

# Purpose of this overwrite (not override) is to depend on and use the shared key vault stub.
data "azurerm_key_vault" "kv_sharedresources" {
  name                = module.kv_shared_stub.name
  resource_group_name = azurerm_resource_group.main.name
  depends_on          = [ module.kv_shared_stub.name ]
}

# Purpose of this overwrite (not override) is to depend on and use the shared key vault stub.
data "azurerm_key_vault_secret" "integration_events_listener_connection_string" {
  name         = local.INTEGRATION_EVENTS_LISTENER_CONNECTION_STRING
  key_vault_id = module.kv_shared_stub.id
  depends_on   = [ module.kvs_integrationevents_listener_connection_string.name ]
}

# Purpose of this overwrite (not override) is to depend on and use the shared key vault stub.
data "azurerm_key_vault_secret" "integration_events_sender_connection_string" {
  name         = local.INTEGRATION_EVENTS_SENDER_CONNECTION_STRING
  key_vault_id = module.kv_shared_stub.id
  depends_on   = [ module.kvs_integrationevents_sender_connection_string.name ]
}

# Purpose of this overwrite (not override) is to depend on and use the shared key vault stub.
data "azurerm_key_vault_secret" "integration_events_manager_connection_string" {
  name         = local.INTEGRATION_EVENTS_MANAGER_CONNECTION_STRING
  key_vault_id = module.kv_shared_stub.id
  depends_on   = [ module.kvs_integrationevents_manager_connection_string.name ]
}

# Purpose of this overwrite (not override) is to depend on and use the shared key vault stub.
data "azurerm_key_vault_secret" "messagehub_storage_connection_string" {
  name         = local.MESSAGEHUB_STORAGE_CONNECTION_STRING_KEY
  key_vault_id = module.kv_shared_stub.id
  depends_on   = [ module.kvs_messagehub_storage_connection_string.name ]
}

# Purpose of this overwrite (not override) is to depend on and use the shared key vault stub.
data "azurerm_key_vault_secret" "messagehub_storage_container" {
  name         = local.MESSAGEHUB_STORAGE_CONTAINER_KEY
  key_vault_id = module.kv_shared_stub.id
  depends_on   = [ module.kvs_messagehub_storage_container.name ]
}
