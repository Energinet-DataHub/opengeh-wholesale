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
module "func_actor_sync" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v9"

  name                                      = "actorsync"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  vnet_integration_subnet_id                = data.azurerm_key_vault_secret.snet_vnet_integrations_id.value
  private_endpoint_subnet_id                = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                       = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  log_analytics_workspace_id                = data.azurerm_key_vault_secret.log_shared_id.value
  health_check_path                         = "/api/monitor/ready"
  always_on                                 = true
  dotnet_framework_version                  = "6"
  use_dotnet_isolated_runtime               = false
  app_settings                              = {
    DB_CONNECTION_STRING                    = local.MS_MARKETROLES_CONNECTION_STRING_SQL_AUTH
    ACTOR_REGISTRY_DB_CONNECTION_STRING     = local.ACTOR_REGISTRY_DB_CONNECTION_STRING
    TIMER_TRIGGER                           = "0 0 * * * *"
  }

  tags                                      = azurerm_resource_group.this.tags
}