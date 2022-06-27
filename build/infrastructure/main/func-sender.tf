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
module "func_sender" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=7.0.0"

  name                                      = "sender"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  vnet_integration_subnet_id                = data.azurerm_key_vault_secret.snet_vnet_integrations_id.value
  private_endpoint_subnet_id                = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                       = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  log_analytics_workspace_id                = data.azurerm_key_vault_secret.log_shared_id.value
  always_on                                 = true
  health_check_path                         = "/api/monitor/ready"
  health_check_alert_action_group_id        = data.azurerm_key_vault_secret.primary_action_group_id.value
  health_check_alert_enabled                = var.enable_health_check_alerts
  dotnet_framework_version                  = "6"
  use_dotnet_isolated_runtime               = true

  app_settings                              = {
    SERVICE_BUS_LISTEN_CONNECTION_STRING             = module.sb_wholesale.primary_connection_strings["listen"]
    SERVICE_BUS_MANAGE_CONNECTION_STRING             = module.sb_wholesale.primary_connection_strings["manage"]
    PROCESS_COMPLETED_TOPIC_NAME                     = module.sbt_completed_process.name
    PROCESS_COMPLETED_SUBSCRIPTION_NAME              = "completed-process-sub-sender"

    # Message Hub
    MESSAGEHUB_STORAGE_CONNECTION_STRING             = data.azurerm_key_vault_secret.messagehub_storage_connection_string.value
    MESSAGEHUB_STORAGE_CONTAINER                     = data.azurerm_key_vault_secret.messagehub_storage_container.value
    MESSAGEHUB_SERVICE_BUS_CONNECTION_STRING         = 
    MESSAGEHUB_DATAAVAILABLE_QUEUE                   = data.azurerm_key_vault_secret.messagehub_dataavailable_queue.value
    MESSAGEHUB_BUNDLEREQUEST_QUEUE                   = data.azurerm_key_vault_secret.messagehub_bundlerequest_queue.value
    MESSAGEHUB_BUNDLEREPLY_QUEUE                     = data.azurerm_key_vault_secret.messagehub_bundlereply_queue.value
  }

  tags                                  = azurerm_resource_group.this.tags
}