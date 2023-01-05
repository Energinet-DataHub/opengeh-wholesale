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
module "st_data_lake" {
  source                            = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=v10"

  name                              = "datalake"
  project_name                      = var.domain_name_short
  environment_short                 = var.environment_short
  environment_instance              = var.environment_instance
  resource_group_name               = azurerm_resource_group.this.name
  location                          = azurerm_resource_group.this.location
  account_replication_type          = "LRS"
  account_tier                      = "Standard"
  is_hns_enabled                    = true
  log_analytics_workspace_id        = module.log_workspace_shared.id
  private_endpoint_subnet_id        = module.snet_private_endpoints.id
  private_dns_resource_group_name   = module.dbw_shared.private_dns_zone_resource_group_name

  tags                              = azurerm_resource_group.this.tags
}

module "kvs_st_data_lake_primary_connection_string" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "st-data-lake-primary-connection-string"
  value         = module.st_data_lake.primary_connection_string
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_st_data_lake_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "st-data-lake-name"
  value         = module.st_data_lake.name
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_st_data_lake_primary_access_key" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "st-data-lake-primary-access-key"
  value         = module.st_data_lake.primary_access_key
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

resource "azurerm_role_assignment" "st_datalake_spn" {
  scope                = module.st_data_lake.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azurerm_client_config.current.object_id
}
