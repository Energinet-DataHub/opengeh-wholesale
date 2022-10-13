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
    DB_CONNECTION_STRING                             = local.DB_CONNECTION_STRING
    
    # Used for health check of all inter domain service bus connections (integration events and Message Hub)
    INTEGRATIONEVENT_MANAGER_CONNECTION_STRING       = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-manage-connection-string)"

    # Service Bus
    SERVICE_BUS_MANAGE_CONNECTION_STRING                         = module.sb_wholesale.primary_connection_strings["manage"]
    SERVICE_BUS_LISTEN_CONNECTION_STRING                         = module.sb_wholesale.primary_connection_strings["listen"]
    DOMAIN_EVENTS_TOPIC_NAME                                     = module.sbt_domain_events.name
    SEND_DATA_AVAILABLE_WHEN_COMPLETED_PROCESS_SUBSCRIPTION_NAME = local.SEND_DATA_AVAILABLE_WHEN_COMPLETED_PROCESS_SUBSCRIPTION_NAME

    # Message Hub
    MESSAGE_HUB_SERVICE_BUS_SEND_CONNECTION_STRING               = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-send-connection-string)"
    MESSAGE_HUB_SERVICE_BUS_LISTEN_CONNECTION_STRING             = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-listen-connection-string)"
    MESSAGE_HUB_DATA_AVAILABLE_QUEUE_NAME                        = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbq-data-available-name)"
    MESSAGE_HUB_REQUEST_QUEUE_NAME                               = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbq-wholesale-name)"
    MESSAGE_HUB_REPLY_QUEUE_NAME                                 = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbq-wholesale-reply-name)"
    MESSAGE_HUB_STORAGE_CONNECTION_STRING                        = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-marketres-primary-connection-string)"
    MESSAGE_HUB_STORAGE_CONTAINER_NAME                           = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-marketres-postofficereply-container-name)"

    CALCULATOR_RESULTS_CONNECTION_STRING                         = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-data-lake-primary-connection-string)"
    CALCULATOR_RESULTS_FILE_SYSTEM_NAME                          = local.PROCESSES_CONTAINER_NAME
  }

  tags                                  = azurerm_resource_group.this.tags
}
