module "app_time_series_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v12"

  name                                     = "timeseriesapi"
  project_name                             = var.domain_name_short
  environment_short                        = var.environment_short
  environment_instance                     = var.environment_instance
  resource_group_name                      = azurerm_resource_group.this.name
  location                                 = azurerm_resource_group.this.location
  vnet_integration_subnet_id               = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id               = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_restriction_allow_ip_range            = var.hosted_deployagent_public_ip_range
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
    "AzureAd__Instance"     = "https://login.microsoftonline.com/"
    "AzureAd__TenantId"     = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=b2c-tenant-id)"
    "AzureAd__ClientId"     = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=eloverblik-timeseriesapi-client-app-id)"
    "AzureAd__ResourceId"   = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-timeseriesapi-app-id)"

    # Databricks
    "DatabricksOptions__WorkspaceToken" = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=dbw-shared-workspace-token)"
    "DatabricksOptions__WorkspaceUrl"   = "https://${data.azurerm_key_vault_secret.dbw_databricks_workspace_url.value}"
    "DatabricksOptions__WarehouseId"    = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-databricks-sql-endpoint-id)"

    # Logging
    "Logging__LogLevel__Default"                      = "Information"
    "Logging__LogLevel__Microsoft.Hosting.Lifetime"   = "Information"
    "Logging__LogLevel__Energinet.Datahub.Migrations" = "Warning"
    "Logging__LogLevel__Energinet.Datahub.Core"       = "Warning"
  }

  role_assignments = [
    {
      resource_id          = data.azurerm_key_vault_secret.st_data_lake_id.value
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
}
