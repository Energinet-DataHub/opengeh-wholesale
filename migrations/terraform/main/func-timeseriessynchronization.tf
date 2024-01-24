module "func_timeseriessynchronization" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v13"

  name                                     = "timeseriessynchronization"
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
  use_dotnet_isolated_runtime              = true
  durable_function                         = true
  health_check_path                        = "/api/monitor/ready"
  health_check_alert = {
    action_group_id = data.azurerm_key_vault_secret.primary_action_group_id.value
    enabled         = var.enable_health_check_alerts
  }
  role_assignments = [
    {
      resource_id          = module.st_dh2data.id
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]

  app_settings = {
    WEBSITE_LOAD_CERTIFICATES                       = local.datahub2_certificate_thumbprint
    StorageAccountSettings__Dh2StorageAccountUri    = "https://${module.st_dh2data.name}.blob.core.windows.net"
    StorageAccountSettings__TimeSeriesContainerName = azurerm_storage_container.dh2_timeseries_synchronization.name
    DataHub2ClientSettings__EndpointAddress         = "https://b2b.te7.datahub.dk",
    FeatureManagement__DataHub2HealthCheck          = var.feature_flag_datahub2_healthcheck

    # Logging Worker
    "Logging__LogLevel__Default"                      = local.LOGGING_LOGLEVEL_WORKER_DEFAULT
    "Logging__LogLevel__Energinet.DataHub.Migrations" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_MIGRATIONS
    "Logging__LogLevel__Energinet.DataHub.Core"       = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE

    # Logging Host
    "AzureFunctionsJobHost__Logging__LogLevel__Default" = local.LOGGING_LOGLEVEL_HOST_DEFAULT
  }
}
