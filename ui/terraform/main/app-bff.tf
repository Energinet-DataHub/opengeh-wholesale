module "backend_for_frontend" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=app-service_7.0.1"

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
  dotnet_framework_version               = "v6.0"
  role_assignments = [
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    }
  ]

  monitor_action_group = length(module.monitor_action_group_ui) != 1 ? null : {
    id                  = module.monitor_action_group_ui[0].id
    resource_group_name = azurerm_resource_group.this.name
  }

  app_settings = {
    #Logging
    "Logging__ApplicationInsights__LogLevel__Default"                = "Information"
    "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core" = "Information"

    ApiClientSettings__MarketParticipantBaseUrl                            = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-markpart-api-base-url)"
    ApiClientSettings__WholesaleBaseUrl                                    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-wholesale-webapi-base-url)"
    ApiClientSettings__WholesaleOrchestrationsBaseUrl                      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-wholesale-orchestrationsdf-base-url)"
    ApiClientSettings__WholesaleOrchestrationSettlementReportsBaseUrl      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-settlement-reports-df-base-url)"
    ApiClientSettings__WholesaleOrchestrationSettlementReportsLightBaseUrl = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-settlement-reports-light-df-base-url)"
    ApiClientSettings__ESettExchangeBaseUrl                                = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-esett-webapi-base-url)"
    ApiClientSettings__EdiB2CWebApiBaseUrl                                 = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-edi-b2cwebapi-base-url)"
    ApiClientSettings__ImbalancePricesBaseUrl                              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-grid-loss-imbalance-prices-api-base-url)"
    ApiClientSettings__SettlementReportsAPIBaseUrl                         = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-settlement-report-webapi-base-url)"
    ApiClientSettings__NotificationsBaseUrl                                = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-notifications-worker-base-url)"
    MITID_EXTERNAL_OPEN_ID_URL                                             = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=mitid-frontend-open-id-url)"
    EXTERNAL_OPEN_ID_URL                                                   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=frontend-open-id-url)"
    INTERNAL_OPEN_ID_URL                                                   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=api-backend-open-id-url)"
    BACKEND_BFF_APP_ID                                                     = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-bff-app-id)"

    "UserAuthentication:MitIdExternalMetadataAddress" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=mitid-frontend-open-id-url)"
    "UserAuthentication:ExternalMetadataAddress"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=frontend-open-id-url)"
    "UserAuthentication:InternalMetadataAddress"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=api-backend-open-id-url)"
    "UserAuthentication:BackendBffAppId"              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-bff-app-id)"
  }
}
