
locals {
  func_settlement_reports_df = {
    app_settings = {
      # Timeout
      "AzureFunctionsJobHost__functionTimeout" = "01:00:00"

      # Logging
      # => Azure Function Worker
      # Explicit override the default "Warning" level filter set by the Application Insights SDK.
      # Otherwise custom logging at "Information" level is not possible, even if we configure it on categories.
      # For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
      "Logging__ApplicationInsights__LogLevel__Default"                     = "Information"
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Wholesale" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_WHOLESALE
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core"      = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE

      # Storage (DataLake)
      STORAGE_CONTAINER_NAME = local.STORAGE_CONTAINER_NAME
      STORAGE_ACCOUNT_URI    = local.STORAGE_ACCOUNT_URI

      # Storage (Blob)
      "SettlementReportStorage__StorageAccountUri"    = local.BLOB_STORAGE_ACCOUNT_URI
      "SettlementReportStorage__StorageContainerName" = local.BLOB_CONTAINER_SETTLEMENTREPORTS_NAME

      # Databricks
      WorkspaceToken        = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-workspace-token)"
      WorkspaceUrl          = "https://${module.dbw.workspace_url}"
      WarehouseId           = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-databricks-sql-endpoint-id)"
      DatabricksCatalogName = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=shared-unity-catalog-name)"

      # Database
      "CONNECTIONSTRINGS__DB_CONNECTION_STRING" = local.DB_CONNECTION_STRING

      # Durable Functions Task Hub Name
      # See naming constraints: https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-task-hubs?tabs=csharp#task-hub-names
      "OrchestrationsTaskHubName" = "Wholesale02"
    }
  }
}
