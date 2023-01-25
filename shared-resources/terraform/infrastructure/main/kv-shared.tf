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
module "kv_shared" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault?ref=v10"

  name                            = "main"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  enabled_for_template_deployment = true
  sku_name                        = "premium"
  log_analytics_workspace_id      = module.log_workspace_shared.id
  private_endpoint_subnet_id      = module.snet_private_endpoints.id
  allowed_subnet_ids              = [
    module.snet_vnet_integrations.id,
    data.azurerm_subnet.deployment_agents_subnet.id
  ]
  ip_rules = var.hosted_deployagent_public_ip_range
}

module "kvs_pir_hosted_deployment_agents" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "pir-hosted-deployment-agents"
  value         = var.hosted_deployagent_public_ip_range
  key_vault_id  = module.kv_shared.id
}