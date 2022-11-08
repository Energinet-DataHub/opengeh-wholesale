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
module "func_receiver" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v9"

  name                                      = "api"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  app_service_plan_id                       = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  log_analytics_workspace_id                = data.azurerm_key_vault_secret.log_shared_id.value
  vnet_integration_subnet_id                = data.azurerm_key_vault_secret.snet_vnet_integrations_id.value
  private_endpoint_subnet_id                = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  always_on                                 = true
  dotnet_framework_version                  = "6"
  use_dotnet_isolated_runtime               = true
  health_check_path                         = "/api/monitor/ready"
  app_settings                              = {
    # Shared resources logging
    REQUEST_RESPONSE_LOGGING_CONNECTION_STRING                    = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-marketoplogs-primary-connection-string)",
    REQUEST_RESPONSE_LOGGING_CONTAINER_NAME                       = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-marketoplogs-container-name)",
    B2C_TENANT_ID                                                 = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=b2c-tenant-id)",
    BACKEND_SERVICE_APP_ID                                        = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-service-app-id)",
    # Endregion: Default Values
    DB_CONNECTION_STRING                                          = local.MS_MARKETROLES_CONNECTION_STRING
    INCOMING_CHANGE_OF_SUPPLIER_MESSAGE_QUEUE_NAME                = module.sbq_incoming_change_supplier_messagequeue.name
    INCOMING_CHANGE_CUSTOMER_CHARACTERISTICS_MESSAGE_QUEUE_NAME   = module.sbq_incoming_change_customer_characteristics_message_queue.name
    MESSAGEHUB_QUEUE_CONNECTION_STRING                            = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-transceiver-connection-string)",
    MESSAGEHUB_DATA_AVAILABLE_QUEUE                               = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbq-data-available-name)",
    MESSAGEHUB_DOMAIN_REPLY_QUEUE                                 = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbq-marketroles-reply-name)",
    MESSAGEHUB_STORAGE_CONTAINER_NAME                             = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-marketres-postofficereply-container-name)",
    MESSAGEHUB_STORAGE_CONNECTION_STRING                          = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-marketres-primary-connection-string)",
    MESSAGE_REQUEST_QUEUE                                         = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbq-marketroles-name)",
    MOVE_IN_REQUEST_ENDPOINT                                      = "https://func-processing-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/MoveIn"
    SERVICE_BUS_CONNECTION_STRING_FOR_DOMAIN_RELAY_LISTENER       = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-listen-connection-string)"
    SERVICE_BUS_CONNECTION_STRING_FOR_DOMAIN_RELAY_MANAGE         = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-manage-connection-string)"
    SERVICE_BUS_CONNECTION_STRING_FOR_DOMAIN_RELAY_SEND           = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-send-connection-string)"
    METERING_POINT_MASTER_DATA_RESPONSE_QUEUE_NAME                = data.azurerm_key_vault_secret.sbq_metering_point_master_data_response_name.value,
    ENERGY_SUPPLIER_CHANGED_TOPIC                                 = data.azurerm_key_vault_secret.sbt_energy_supplier_changed_name.value,
    ENERGY_SUPPLIER_CHANGED_SUBSCRIPTION                          = data.azurerm_key_vault_secret.sbs_energy_supplier_changed_to_messaging_name.value
    MASTER_DATA_REQUEST_QUEUE_NAME                                = data.azurerm_key_vault_secret.sbq_metering_point_master_data_request_name.value,
    CUSTOMER_MASTER_DATA_RESPONSE_QUEUE_NAME                      = module.sbq_customermasterdataresponsequeue.name
    CUSTOMER_MASTER_DATA_REQUEST_QUEUE_NAME                       = module.sbq_customermasterdatarequestqueue.name
    INTEGRATION_EVENT_TOPIC_NAME                                  = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-sharedres-integrationevent-received-name)"
    CONSUMER_MOVED_IN_EVENT_SUBSCRIPTION_NAME                     = module.sbs_consumer_moved_in.name
    ENERGY_SUPPLIER_CHANGED_EVENT_SUBSCRIPTION_NAME               = module.sbs_energy_supplier_changed.name
    MARKET_PARTICIPANT_CHANGED_ACTOR_CREATED_SUBSCRIPTION_NAME    = module.sbs_market_roles_b2b_actor_created.name
    METERING_POINT_CREATED_EVENT_B2B_SUBSCRIPTION_NAME            = module.sbs_metering_point_created_b2b_event.name
    CREATE_METERING_POINT_TRANSACTIONS_QUEUE_NAME                 = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbq-create-metering-point-transactions)"
    CUSTOMER_MASTER_DATA_UPDATE_RESPONSE_QUEUE_NAME               = module.sbq_customermasterdataupdateresponsequeue.name
  }

  tags = azurerm_resource_group.this.tags
}
