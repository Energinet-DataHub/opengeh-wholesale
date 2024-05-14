module "bff" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v13"

  name                                     = "bff"
  project_name                             = var.domain_name_short
  environment_short                        = var.environment_short
  environment_instance                     = var.environment_instance
  resource_group_name                      = azurerm_resource_group.this.name
  location                                 = azurerm_resource_group.this.location
  vnet_integration_subnet_id               = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id               = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_restrictions                          = var.ip_restrictions
  scm_ip_restrictions                      = var.ip_restrictions
  app_service_plan_id                      = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  health_check_path                        = "/monitor/ready"
  health_check_alert_action_group_id       = data.azurerm_key_vault_secret.primary_action_group_id.value
  health_check_alert_enabled               = var.enable_health_check_alerts
  dotnet_framework_version                 = "v6.0"
  role_assignments = [
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    }
  ]
  app_settings = {
    ApiClientSettings__MarketParticipantBaseUrl       = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-markpart-api-base-url)"
    ApiClientSettings__WholesaleBaseUrl               = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-wholesale-webapi-base-url)"
    ApiClientSettings__WholesaleOrchestrationsBaseUrl = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-wholesale-orchestrationsdf-base-url)"
    ApiClientSettings__ESettExchangeBaseUrl           = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-esett-webapi-base-url)"
    ApiClientSettings__EdiB2CWebApiBaseUrl            = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-edi-b2cwebapi-base-url)"
    ApiClientSettings__ImbalancePricesBaseUrl         = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-grid-loss-imbalance-prices-api-base-url)"
    MITID_EXTERNAL_OPEN_ID_URL                        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=mitid-frontend-open-id-url)"
    EXTERNAL_OPEN_ID_URL                              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=frontend-open-id-url)"
    INTERNAL_OPEN_ID_URL                              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=api-backend-open-id-url)"
    BACKEND_BFF_APP_ID                                = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-bff-app-id)"
  }
}

#TODO: Point healthcheckUI endpoints to this resource in step 2
#TODO: Create database migration in Sauron to delete the old resource - also in step 2
#TODO: Remove old servicedeploy from dh3-environments in step 2
module "backend_for_frontend" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v14"

  name                                   = "backend-for-frontend"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  app_service_plan_id                    = module.webapp_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  health_check_path                      = "/monitor/ready"
  health_check_alert_action_group_id     = data.azurerm_key_vault_secret.primary_action_group_id.value
  health_check_alert_enabled             = var.enable_health_check_alerts
  dotnet_framework_version               = "v6.0"
  role_assignments = [
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    }
  ]
  app_settings = {
    ApiClientSettings__MarketParticipantBaseUrl       = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-markpart-api-base-url)"
    ApiClientSettings__WholesaleBaseUrl               = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-wholesale-webapi-base-url)"
    ApiClientSettings__WholesaleOrchestrationsBaseUrl = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-wholesale-orchestrationsdf-base-url)"
    ApiClientSettings__ESettExchangeBaseUrl           = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-esett-webapi-base-url)"
    ApiClientSettings__EdiB2CWebApiBaseUrl            = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-edi-b2cwebapi-base-url)"
    ApiClientSettings__ImbalancePricesBaseUrl         = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-grid-loss-imbalance-prices-api-base-url)"
    MITID_EXTERNAL_OPEN_ID_URL                        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=mitid-frontend-open-id-url)"
    EXTERNAL_OPEN_ID_URL                              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=frontend-open-id-url)"
    INTERNAL_OPEN_ID_URL                              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=api-backend-open-id-url)"
    BACKEND_BFF_APP_ID                                = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-bff-app-id)"
  }
}
