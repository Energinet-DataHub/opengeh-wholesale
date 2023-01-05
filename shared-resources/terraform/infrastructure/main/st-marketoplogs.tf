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
locals {
  marketoplogs_container_name = "marketoplogs"
  marketoplogsarchive_container_name = "marketoplogs-archive"
}

module "st_market_operator_logs" {
  source                            = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=v10"

  name                        = "marketlog"
  project_name                = var.domain_name_short
  environment_short           = var.environment_short
  environment_instance        = var.environment_instance
  resource_group_name         = azurerm_resource_group.this.name
  location                    = azurerm_resource_group.this.location
  account_replication_type    = "LRS"
  access_tier                 = "Hot"
  account_tier                = "Standard"
  log_analytics_workspace_id  = module.log_workspace_shared.id
  private_endpoint_subnet_id  = module.snet_private_endpoints.id
  
  containers                  = [
    {
      name = local.marketoplogs_container_name,
    },
    {
      name = local.marketoplogsarchive_container_name,
    },
  ]

  tags                        = azurerm_resource_group.this.tags
}

module "kvs_st_market_operator_logs_primary_connection_string" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"
  
  name          = "st-marketoplogs-primary-connection-string"
  value         = module.st_market_operator_logs.primary_connection_string
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_st_market_operator_logs_container_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "st-marketoplogs-container-name"
  value         = local.marketoplogs_container_name
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_st_market_operator_logs_archive_container_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "st-marketoplogs-archive-container-name"
  value         = local.marketoplogsarchive_container_name
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}
