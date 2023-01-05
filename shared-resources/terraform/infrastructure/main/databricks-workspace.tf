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
module "dbw_shared" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-workspace?ref=v10"

  name                                      = "dbw"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  sku                                       = "premium"
  main_virtual_network_id                   = data.azurerm_virtual_network.this.id
  main_virtual_network_name                 = data.azurerm_virtual_network.this.name
  main_virtual_network_resource_group_name  = data.azurerm_virtual_network.this.resource_group_name
  databricks_virtual_network_address_space  = var.databricks_vnet_address_space
  private_subnet_address_prefix             = var.databricks_private_subnet_address_prefix
  public_subnet_address_prefix              = var.databricks_public_subnet_address_prefix
  public_network_service_endpoints          = [
    "Microsoft.EventHub"
  ]

  log_analytics_workspace_id                = module.log_workspace_shared.id

  tags                                      = azurerm_resource_group.this.tags
}

module "kvs_databricks_workspace_id" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "dbw-shared-workspace-id"
  value         = module.dbw_shared.id
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_databricks_workspace_url" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "dbw-shared-workspace-url"
  value         = module.dbw_shared.workspace_url
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_databricks_public_network_id" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "dbw-public-network-id"
  value         = module.dbw_shared.public_network_id
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_databricks_private_dns_resource_group_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "databricks-private-dns-resource-group-name"
  value         = module.dbw_shared.private_dns_zone_resource_group_name
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

data "external" "databricks_token" {
  program = ["pwsh", "${path.cwd}/scripts/generate-pat-token.ps1", module.dbw_shared.id, "https://${module.dbw_shared.workspace_url}"]
  depends_on = [
    module.dbw_shared
  ]
}

module "kvs_databricks_dbw_shared_workspace_token" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "dbw-shared-workspace-token"
  value         = data.external.databricks_token.result.pat_token
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}