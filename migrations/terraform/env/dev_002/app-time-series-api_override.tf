module "app_time_series_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v13"

  app_settings = {
    "TimeZone" = "Europe/Copenhagen"

    # Azure AD
    "AzureAd__Instance"   = "https://login.microsoftonline.com/"
    "AzureAd__TenantId"   = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=b2c-tenant-id)"
    "AzureAd__ClientId"   = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=eloverblik-timeseriesapi-client-app-id)"
    "AzureAd__ResourceId" = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-timeseriesapi-app-id)"

    # Databricks
    "DatabricksOptions__WorkspaceToken"       = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-workspace-token)"
    "DatabricksOptions__WarehouseId"          = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-databricks-sql-endpoint-id)"
    "DatabricksOptions__WorkspaceUrl"         = "https://${module.dbw.workspace_url}"
    "DatabricksOptions__HealthCheckStartHour" = 5
    "DatabricksOptions__HealthCheckEndHour"   = 16

    # Logging
    "Logging__ApplicationInsights__LogLevel__Default"                      = "Warning"
    "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Migrations" = "Information"
    "Logging__ApplicationInsights__LogLevel__Energinet.Datahub.Core"       = "Information"
  }
}
