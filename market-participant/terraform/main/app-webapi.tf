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
module "app_webapi" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v9"

  name                                      = "webapi"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  vnet_integration_subnet_id                = data.azurerm_key_vault_secret.snet_vnet_integrations_id.value
  private_endpoint_subnet_id                = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                       = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  health_check_path                         = "/monitor/ready"
  health_check_alert_action_group_id        = data.azurerm_key_vault_secret.primary_action_group_id.value
  health_check_alert_enabled                = var.enable_health_check_alerts
  dotnet_framework_version                  = "v6.0"

  app_settings                              = {
    EXTERNAL_OPEN_ID_URL                       = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=frontend-open-id-url)"
    INTERNAL_OPEN_ID_URL                       = "https://app-webapi-${var.domain_name_short}-${var.environment_short}-${var.environment_instance}.azurewebsites.net/.well-known/openid-configuration"
    BACKEND_SERVICE_APP_ID                     = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=frontend-service-app-id)"
    SQL_MP_DB_CONNECTION_STRING                = local.MS_MARKET_PARTICIPANT_CONNECTION_STRING
    SERVICE_BUS_CONNECTION_STRING              = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-send-connection-string)"
    SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-manage-connection-string)"
    SBT_MARKET_PARTICIPANT_CHANGED_NAME        = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-sharedres-integrationevent-received-name)"
    AZURE_B2C_TENANT                           = var.b2c_tenant
    AZURE_B2C_SPN_ID                           = var.b2c_spn_id
    AZURE_B2C_SPN_SECRET                       = var.b2c_spn_secret
    AZURE_B2C_BACKEND_OBJECT_ID                = var.b2c_backend_object_id
    AZURE_B2C_BACKEND_SPN_OBJECT_ID            = var.b2c_backend_spn_object_id
    AZURE_B2C_BACKEND_ID                       = var.b2c_backend_id
    TOKEN_KEY_VAULT                            = data.azurerm_key_vault.kv_shared_resources.vault_uri
    TOKEN_KEY_NAME                             = azurerm_key_vault_key.token_sign.name
  }

  tags                                       = azurerm_resource_group.this.tags
}

module "kvs_app_markpart_webapi_base_url" {
  source                                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=7.0.0"

  name                                            = "app-markpart-webapi-base-url"
  value                                           = "https://${module.app_webapi.default_hostname}"
  key_vault_id                                    = data.azurerm_key_vault.kv_shared_resources.id

  tags                                            = azurerm_resource_group.this.tags
}

module "kvs_backend_open_id_url" {
  source                                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=7.0.0"

  name                                            = "backend-open-id-url"
  value                                           = "https://${module.app_webapi.default_hostname}/.well-known/openid-configuration"
  key_vault_id                                    = data.azurerm_key_vault.kv_shared_resources.id

  tags                                            = azurerm_resource_group.this.tags
}
