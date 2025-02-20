
locals {
  app_measurements_api = {
    app_settings = {
      "TimeZone" = "Europe/Copenhagen"

      # Databricks
      "DatabricksOptions__WorkspaceToken"       = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-workspace-token)"
      "DatabricksOptions__WarehouseId"          = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-databricks-measurements-api-sql-endpoint-id)"
      "DatabricksOptions__WorkspaceUrl"         = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-workspace-url)"
      "DatabricksOptions__HealthCheckStartHour" = 5
      "DatabricksOptions__HealthCheckEndHour"   = 16

      # Unity Catalog
      "CatalogOptions__CatalogName" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=shared-unity-catalog-name)"

      # Logging
      "Logging__ApplicationInsights__LogLevel__Default"                            = local.LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.MeasurementsCore" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_MEASUREMENTS_CORE
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core"             = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE
    }
  }
}
