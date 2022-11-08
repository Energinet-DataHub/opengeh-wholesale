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
module "func_outbox" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v9"

  name                                      = "outbox"
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
  dotnet_framework_version                  = "6"
  use_dotnet_isolated_runtime               = true
  health_check_path                         = "/api/monitor/ready"

  app_settings                              = {
    METERINGPOINT_DB_CONNECTION_STRING                            = local.MS_METERING_POINT_CONNECTION_STRING

    SHARED_SERVICE_BUS_SENDER_CONNECTION_STRING                   = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-send-connection-string)"
    SHARED_SERVICE_BUS_MANAGE_CONNECTION_STRING                   = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-manage-connection-string)"

    INTEGRATION_EVENT_TOPIC_NAME                                  = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-sharedres-integrationevent-received-name)"
    METERING_POINT_CREATED_TOPIC                                  = "metering-point-created"
    METERING_POINT_CONNECTED_TOPIC                                = "metering-point-connected"
    METERING_POINT_MESSAGE_DEQUEUED_TOPIC                         = "metering-point-message-dequeued"
    ACTOR_MESSAGE_DISPATCH_TRIGGER_TIMER                          = "*/10 * * * * *"
    EVENT_MESSAGE_DISPATCH_TRIGGER_TIMER                          = "*/10 * * * * *"

    MESSAGEHUB_STORAGE_CONNECTION_STRING                          = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-marketres-primary-connection-string)"
    MESSAGEHUB_QUEUE_CONNECTION_STRING                            = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-transceiver-connection-string)"
    MESSAGEHUB_STORAGE_CONTAINER_NAME                             = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-marketres-postofficereply-container-name)"
    CHARGES_DEFAULT_LINK_RESPONSE_QUEUE                           = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbq-create-link-reply-name)"
    MESSAGEHUB_DATA_AVAILABLE_QUEUE                               = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbq-data-available-name)"
    MESSAGEHUB_DOMAIN_REPLY_QUEUE                                 = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbq-metering-points-reply-name)"
  }

  tags                                      = azurerm_resource_group.this.tags
}