module "app_time_series_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v13"

  name                                     = "timeseriesapi"
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
  application_insights_instrumentation_key = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  always_on                                = true
  dotnet_framework_version                 = "v7.0"
  health_check_path                        = "/monitor/ready"
  health_check_alert_action_group_id       = data.azurerm_key_vault_secret.primary_action_group_id.value
  health_check_alert_enabled               = var.enable_health_check_alerts

  app_settings = {
    "TimeZone" = "Europe/Copenhagen"

    # Azure AD
    "AzureAd__Instance"   = "https://login.microsoftonline.com/"
    "AzureAd__TenantId"   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=b2c-tenant-id)"
    "AzureAd__ClientId"   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=eloverblik-timeseriesapi-client-app-id)"
    "AzureAd__ResourceId" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-timeseriesapi-app-id)"

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
  role_assignments = [
    {
      resource_id          = data.azurerm_key_vault_secret.st_data_lake_id.value
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
}
