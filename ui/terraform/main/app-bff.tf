module "bff" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v12"

  name                                     = "bff"
  project_name                             = var.domain_name_short
  environment_short                        = var.environment_short
  environment_instance                     = var.environment_instance
  resource_group_name                      = azurerm_resource_group.this.name
  location                                 = azurerm_resource_group.this.location
  vnet_integration_subnet_id               = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id               = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_restriction_allow_ip_range            = var.hosted_deployagent_public_ip_range
  app_service_plan_id                      = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  health_check_path                        = "/monitor/ready"
  health_check_alert_action_group_id       = data.azurerm_key_vault_secret.primary_action_group_id.value
  health_check_alert_enabled               = var.enable_health_check_alerts
  dotnet_framework_version                 = "v6.0"

  app_settings = {
    ApiClientSettings__MessageArchiveBaseUrl    = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=func-edi-api-base-url)"
    ApiClientSettings__MeteringPointBaseUrl     = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=app-metering-point-webapi-base-url)"
    ApiClientSettings__ChargesBaseUrl           = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=app-charges-webapi-base-url)"
    ApiClientSettings__MarketParticipantBaseUrl = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=app-markpart-webapi-base-url)"
    ApiClientSettings__WholesaleBaseUrl         = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=app-wholesale-api-base-url)"
    ApiClientSettings__ESettExchangeBaseUrl     = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=app-esett-webapi-base-url)"
    ApiClientSettings__EdiB2CWebApiBaseUrl      = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=app-edi-b2cwebapi-base-url)"
    EXTERNAL_OPEN_ID_URL                        = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=frontend-open-id-url)"
    INTERNAL_OPEN_ID_URL                        = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-open-id-url)"
    BACKEND_BFF_APP_ID                          = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-bff-app-id)"
  }
}
