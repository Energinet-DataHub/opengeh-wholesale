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

resource "azurerm_storage_blob" "azurerm_storage_blob" {
  name                    = "${local.DATA_LAKE_TIMESERIES_UNPROCESSED_FOLDER_NAME}/notused"
  storage_account_name    = data.azurerm_key_vault_secret.st_shared_data_lake_name.value
  storage_container_name  = local.DATA_LAKE_CONTAINER_NAME
  type                    = "Block"
}

resource "azurerm_storage_container" "container" {
  name                  = local.DATA_LAKE_CONTAINER_NAME
  storage_account_name  = data.azurerm_key_vault_secret.st_shared_data_lake_name.value
}

module "kvs_st_data_lake_container_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=7.0.0"

  name          = "st-data-lake-timeseries-container-name"
  value         = local.DATA_LAKE_CONTAINER_NAME
  key_vault_id  = data.azurerm_key_vault.kv_shared_resources.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_st_data_lake_timeseries_unprocessed_folder_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=7.0.0"

  name          = "st-data-lake-timeseries-unprocessed-blob-name"
  value         = local.DATA_LAKE_TIMESERIES_UNPROCESSED_FOLDER_NAME
  key_vault_id  = data.azurerm_key_vault.kv_shared_resources.id

  tags          = azurerm_resource_group.this.tags
}
