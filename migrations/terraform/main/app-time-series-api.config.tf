
locals {
  app_time_series_api = {
    app_settings = {
      "TimeZone" = "Europe/Copenhagen"

      # Azure AD
      "AzureAd__Instance"   = "https://login.microsoftonline.com/"
      "AzureAd__TenantId"   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=b2c-tenant-id)"
      "AzureAd__ClientId"   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=eloverblik-timeseriesapi-client-app-id)"
      "AzureAd__ResourceId" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-timeseriesapi-app-id)"

      # Databricks
      "DatabricksOptions__WorkspaceToken"       = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-workspace-token)"
      "DatabricksOptions__WarehouseId"          = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-databricks-ts-api-sql-endpoint-id)"
      "DatabricksOptions__WorkspaceUrl"         = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-workspace-url)"
      "DatabricksOptions__HealthCheckStartHour" = 5
      "DatabricksOptions__HealthCheckEndHour"   = 16

      # Unity Catalog
      "CatalogOptions__CatalogName" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=shared-unity-catalog-name)"

      # Logging
      "Logging__ApplicationInsights__LogLevel__Default"                      = local.LOGGING_LOGLEVEL_DEFAULT
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Migrations" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_MIGRATIONS
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core"       = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE
    }
  }
}
