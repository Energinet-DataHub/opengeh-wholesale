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
data "azurerm_key_vault" "kv_shared" {
  name                = var.shared_resources_keyvault_name
  resource_group_name = var.shared_resources_resource_group_name
}

data "azurerm_key_vault_secret" "dbw_databricks_workspace_id" {
  name         = "dbw-shared-workspace-id"
  key_vault_id = data.azurerm_key_vault.kv_shared.id
}

data "azurerm_key_vault_secret" "dbw_databricks_workspace_url" {
  name         = "dbw-shared-workspace-url"
  key_vault_id = data.azurerm_key_vault.kv_shared.id
}

data "azurerm_key_vault_secret" "st_data_lake_name" {
  name         = "st-data-lake-name"
  key_vault_id = data.azurerm_key_vault.kv_shared.id
}

data "azurerm_key_vault_secret" "st_data_lake_primary_access_key" {
  name         = "st-data-lake-primary-access-key"
  key_vault_id = data.azurerm_key_vault.kv_shared.id
}

