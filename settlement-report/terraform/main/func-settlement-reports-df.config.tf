
locals {
  func_settlement_reports_df = {
    app_settings = {
      # Timeout
      "AzureFunctionsJobHost__functionTimeout" = "11:00:00"

      # Storage (Blob)
      "SettlementReportStorage__StorageAccountUri"    = local.BLOB_STORAGE_ACCOUNT_URI
      "SettlementReportStorage__StorageContainerName" = local.BLOB_CONTAINER_SETTLEMENTREPORTS_NAME
      "SettlementReportStorage__StorageAccountForJobsUri"    = local.BLOB_STORAGE_ACCOUNT_JOBS_URI
      "SettlementReportStorage__StorageContainerForJobsName" = local.BLOB_CONTAINER_JOBS_SETTLEMENTREPORTS_NAME

      # Databricks
      WorkspaceToken 			= "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-wholesale-workspace-token)"
      WorkspaceUrl   			= "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-wholesale-workspace-url)"
      WarehouseId    			= "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-settlement-report-sql-endpoint-id)"
      DatabricksCatalogName 	  	= "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=shared-unity-catalog-name)"

      # Database
      "CONNECTIONSTRINGS__DB_CONNECTION_STRING" = local.DB_CONNECTION_STRING

      # Durable Functions Task Hub Name
      # See naming constraints: https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-task-hubs?tabs=csharp#task-hub-names
      "OrchestrationsTaskHubName" = "SettlementReportTaskHub"
    }
  }
}
