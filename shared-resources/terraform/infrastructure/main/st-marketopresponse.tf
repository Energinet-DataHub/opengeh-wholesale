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
  postoffice_reply_container_name = "postoffice-reply"
}

module "st_market_operator_response" {
  source                            = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=v10"

  name                        = "marketres"
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
      name = local.postoffice_reply_container_name,
    },
    {
      name = "timeseries-postoffice-reply",
    },
    {
      name = "charges-postoffice-reply",
    },
    {
      name = "marketroles-postoffice-reply",
    },
    {
      name = "aggregations-postoffice-reply",
    },
    {
      name = "meteringpoints-postoffice-reply",
    },
  ]

  tags                              = azurerm_resource_group.this.tags
}

module "kvs_st_market_operator_response_primary_connection_string" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"
  name          = "st-marketres-primary-connection-string"
  value         = module.st_market_operator_response.primary_connection_string
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_st_market_operator_response_postofficereply_container_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "st-marketres-postofficereply-container-name"
  value         = local.postoffice_reply_container_name
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}
