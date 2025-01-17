locals {
  backend_for_frontend = {
    app_settings = {
      #Logging
      "Logging__ApplicationInsights__LogLevel__Default"                = "Information"
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core" = "Information"

      # Process Manager
      FeatureManagement__UseProcessManager                    = var.feature_management_use_process_manager
      ProcessManagerHttpClients__GeneralApiBaseAddress        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-api-pmcore-base-url)"
      ProcessManagerHttpClients__OrchestrationsApiBaseAddress = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-orchestrations-pmorch-base-url)"

      SubSystemBaseUrls__MarketParticipantBaseUrl                            = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-markpart-api-base-url)"
      SubSystemBaseUrls__WholesaleBaseUrl                                    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-wholesale-webapi-base-url)"
      SubSystemBaseUrls__WholesaleOrchestrationsBaseUrl                      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-wholesale-orchestrationsdf-base-url)"
      SubSystemBaseUrls__WholesaleOrchestrationSettlementReportsBaseUrl      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-settlement-reports-df-base-url)"
      SubSystemBaseUrls__WholesaleOrchestrationSettlementReportsLightBaseUrl = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-settlement-reports-light-df-base-url)"
      SubSystemBaseUrls__ESettExchangeBaseUrl                                = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-esett-webapi-base-url)"
      SubSystemBaseUrls__EdiB2CWebApiBaseUrl                                 = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-edi-b2cwebapi-base-url)"
      SubSystemBaseUrls__ImbalancePricesBaseUrl                              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-grid-loss-imbalance-prices-api-base-url)"
      SubSystemBaseUrls__SettlementReportsAPIBaseUrl                         = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-settlement-report-webapi-base-url)"
      SubSystemBaseUrls__NotificationsBaseUrl                                = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-notifications-worker-base-url)"
      SubSystemBaseUrls__Dh2BridgeBaseUrl                                    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=app-grid-loss-event-receiver-api-base-url)"
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
}
