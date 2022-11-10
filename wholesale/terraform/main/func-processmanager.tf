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
module "func_processmanager" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v9"

  name                                      = "processmanager"
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
  tags                                      = azurerm_resource_group.this.tags

  app_settings                              = {
    # Database
    DB_CONNECTION_STRING                                               = local.DB_CONNECTION_STRING

    CALCULATION_STORAGE_CONNECTION_STRING                              = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-data-lake-primary-connection-string)"
    CALCULATION_STORAGE_CONTAINER_NAME                                 = local.CALCULATION_STORAGE_CONTAINER_NAME
    STORAGE_CONTAINER_NAME                                             = local.STORAGE_CONTAINER_NAME

    # Service bus
    SERVICE_BUS_SEND_CONNECTION_STRING                                 = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-send-connection-string)"
    SERVICE_BUS_LISTEN_CONNECTION_STRING                               = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-listen-connection-string)"
    SERVICE_BUS_MANAGE_CONNECTION_STRING                               = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-manage-connection-string)"
    DOMAIN_EVENTS_TOPIC_NAME                                           = module.sbt_domain_events.name
    ZIP_BASIS_DATA_WHEN_COMPLETED_BATCH_SUBSCRIPTION_NAME              = module.sbtsub_zip_basis_data_when_batch_completed.name
    PUBLISH_PROCESSES_COMPLETED_WHEN_COMPLETED_BATCH_SUBSCRIPTION_NAME = module.sbtsub_publish_process_completed_when_batch_completed.name
    
    # Databricks
    DATABRICKS_WORKSPACE_TOKEN                                         = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=dbw-shared-workspace-token)"
    DATABRICKS_WORKSPACE_URL                                           = "https://${data.azurerm_key_vault_secret.dbw_databricks_workspace_url.value}"

    # Domain events
    BATCH_COMPLETED_EVENT_NAME                                         = local.BATCH_COMPLETED_EVENT_NAME
    PROCESS_COMPLETED_EVENT_NAME                                       = local.PROCESS_COMPLETED_EVENT_NAME
  }
}
