locals {
  backend_for_frontend = {
    app_settings = {
      #Logging
      "Logging__ApplicationInsights__LogLevel__Default"                = "Information"
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core" = "Information"

      # Process Manager
      FeatureManagement__UseProcessManager = var.feature_management_use_process_manager
      # => Old section name, will be removed in a separate PR
      ProcessManagerClient__GeneralApiBaseAddress        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-pm-api-edi-base-url)"
      ProcessManagerClient__OrchestrationsApiBaseAddress = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-pm-orchestrations-edi-base-url)"
      # => New section name
      ProcessManagerHttpClients__GeneralApiBaseAddress        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-pm-api-edi-base-url)"
      ProcessManagerHttpClients__OrchestrationsApiBaseAddress = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-pm-orchestrations-edi-base-url)"

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
}
