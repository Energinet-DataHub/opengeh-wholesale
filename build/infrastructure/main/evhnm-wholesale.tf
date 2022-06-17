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

module "evhnm_wholesale" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/eventhub-namespace?ref=7.0.0"

  name                            = "wholesale"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  sku                             = "Standard"
  capacity                        = 1
  log_analytics_workspace_id      = data.azurerm_key_vault_secret.log_shared_id.value
  private_endpoint_subnet_id      = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  network_ruleset                 = {
    allowed_subnet_ids              = [
      data.azurerm_key_vault_secret.snet_vnet_integrations_id.value,
    ]
  }
  private_dns_resource_group_name = var.shared_resources_resource_group_name

  tags                            = azurerm_resource_group.this.tags
}

module "evh_masterdataevents" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/eventhub?ref=7.0.0"

  name                      = "masterdataevents"
  namespace_name            = module.evhnm_wholesale.name
  resource_group_name       = azurerm_resource_group.this.name
  partition_count           = 4
  message_retention         = 1
  auth_rules            = [
    {
      name    = "send",
      send    = true
    },
    {
      name    = "manage",
      manage  = true
      listen  = true
      send    = true
    },
  ]
}