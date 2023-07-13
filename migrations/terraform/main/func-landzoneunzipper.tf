module "func_landzoneunzipper" {
  source                                                  = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v12"

  name                                                    = "landzoneunzipper"
  project_name                                            = var.domain_name_short
  environment_short                                       = var.environment_short
  environment_instance                                    = var.environment_instance
  resource_group_name                                     = azurerm_resource_group.this.name
  location                                                = azurerm_resource_group.this.location
  vnet_integration_subnet_id                              = data.azurerm_key_vault_secret.snet_vnet_integrations_id.value
  private_endpoint_subnet_id                              = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                                     = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key                = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  always_on                                               = true
  dotnet_framework_version                                = "v7.0"
  use_dotnet_isolated_runtime                             = true
  health_check_path                                       = "/api/monitor/ready"
  health_check_alert                                      = {
    action_group_id                                       = data.azurerm_key_vault_secret.primary_action_group_id.value
    enabled                                               = var.enable_health_check_alerts
  }
  app_settings                                            = {
  WEBSITE_ENABLE_SYNC_UPDATE_SITE                       = true
  WEBSITE_RUN_FROM_PACKAGE                              = 1
  WEBSITES_ENABLE_APP_SERVICE_STORAGE                   = true
  FUNCTIONS_WORKER_RUNTIME                              = "dotnet-isolated"
  # Storage Account and container settings
  ARCHIVE_CONNECTION_STRING                             = "https://${module.st_dh2data.name}.blob.core.windows.net"
  ARCHIVE_CONTAINER_NAME                                = azurerm_storage_container.dh2_metering_point_history.name
  ZIPPED_LANDING_ZONE_CONNECTION_STRING                 = "https://${module.st_dh2data.name}.blob.core.windows.net"
  ZIPPED_CONTAINER_NAME                                 = azurerm_storage_container.dh2_metering_point_history.name
  UNZIPPED_LANDING_ZONE_CONNECTION_STRING               = "https://${module.st_dh2data.name}.blob.core.windows.net"
  UNZIPPED_METERING_POINTS_CONTAINER_NAME               = azurerm_storage_container.dh2_metering_point_history.name
  UNZIPPED_TIME_SERIES_CONTAINER_NAME                   = azurerm_storage_container.dh2_timeseries.name
  UNZIPPED_CHARGES_CONTAINER_NAME                       = azurerm_storage_container.dh2_charges.name
  UNZIPPED_CHARGE_LINKS_CONTAINER_NAME                  = azurerm_storage_container.dh2_charge_links.name
  # Event Hub settings
  INGRESS_EVENT_HUB_CONNECTION_STRING                   = module.eventhub_landzone_zipped.primary_connection_strings["eh-landzone-listener-connection-string"]
  INGRESS_EVENT_HUB_NAME                                = module.eventhub_landzone_zipped.name
  }
  # Role assigments is needed to connect to the storage accounts using URI
  role_assignments = [
    {
      resource_id          = module.st_dh2data.id
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
  depends_on = [
    module.st_dh2landzone,
    module.st_dh2landzone_archive,
    module.st_dh2data,
  ]
}

#---- Data assignments

data "azurerm_storage_account" "st_dh2landzone" {
  name                = module.st_dh2landzone.name
  resource_group_name = azurerm_resource_group.this.name
  depends_on = [
    module.st_dh2landzone,
  ]
}

data "azurerm_storage_account" "st_dh2landzone_archive" {
  name                = module.st_dh2landzone_archive.name
  resource_group_name = azurerm_resource_group.this.name
  depends_on = [
    module.st_dh2landzone_archive,
  ]
}

data "azurerm_storage_account" "st_dh2data" {
  name                = module.st_dh2data.name
  resource_group_name = azurerm_resource_group.this.name
  depends_on = [
    module.st_dh2data,
  ]
}
