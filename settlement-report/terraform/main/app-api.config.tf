locals {
  app_api = {
    app_settings = {
      # Authentication
      "UserAuthentication__MitIdExternalMetadataAddress" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=mitid-frontend-open-id-url)"
      "UserAuthentication__ExternalMetadataAddress"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=frontend-open-id-url)"
      "UserAuthentication__InternalMetadataAddress"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=api-backend-open-id-url)"
      "UserAuthentication__BackendBffAppId"              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-bff-app-id)"
	  
	  # Storage (Blob)
      "SettlementReportStorage__StorageAccountUri"    = local.BLOB_STORAGE_ACCOUNT_URI
      "SettlementReportStorage__StorageContainerName" = local.BLOB_CONTAINER_SETTLEMENTREPORTS_NAME

      # Databricks
      WorkspaceToken 			= "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-wholesale-workspace-token)"
      WorkspaceUrl   			= "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-wholesale-workspace-url)"
      WarehouseId    			= "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-settlement-report-sql-endpoint-id)"
      DatabricksCatalogName 	  	= "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=shared-unity-catalog-name)"
    }
    connection_strings = [
      {
        name  = "DB_CONNECTION_STRING"
        type  = "SQLAzure"
        value = local.DB_CONNECTION_STRING
      }
    ]
  }
}
