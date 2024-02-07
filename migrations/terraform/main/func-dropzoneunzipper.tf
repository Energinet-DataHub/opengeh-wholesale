module "func_dropzoneunzipper" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v13"

  name                                     = "dropzoneunzipper"
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
  health_check_path                        = "/api/monitor/ready"
  use_32_bit_worker                        = false
  health_check_alert = {
    action_group_id = data.azurerm_key_vault_secret.primary_action_group_id.value
    enabled         = var.enable_health_check_alerts
  }
  app_settings = {
    WEBSITE_ENABLE_SYNC_UPDATE_SITE     = true
    WEBSITE_RUN_FROM_PACKAGE            = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = true
    FUNCTIONS_WORKER_RUNTIME            = "dotnet-isolated"

    # Storage Account and container settings
    ARCHIVE_DROPZONE_URI                    = "https://${module.st_dh2dropzone_archive.name}.blob.core.windows.net"
    ARCHIVE_CONTAINER_NAME                  = azurerm_storage_container.dropzonearchive.name
    ZIPPED_DROPZONE_URI                     = "https://${module.st_dh2dropzone.name}.blob.core.windows.net"
    ZIPPED_CONTAINER_NAME                   = azurerm_storage_container.dh2_dropzone_zipped.name
    UNZIPPED_DROPZONE_URI                   = "https://${module.st_dh2data.name}.blob.core.windows.net"
    UNZIPPED_METERING_POINTS_CONTAINER_NAME = azurerm_storage_container.dh2_metering_point_history.name
    UNZIPPED_TIME_SERIES_CONTAINER_NAME     = azurerm_storage_container.dh2_timeseries.name
    UNZIPPED_CHARGES_CONTAINER_NAME         = azurerm_storage_container.dh2_charges.name
    UNZIPPED_CHARGE_LINKS_CONTAINER_NAME    = azurerm_storage_container.dh2_charge_links.name

    # Event Hub settings
    INGRESS_EVENT_HUB_CONNECTION_STRING = azurerm_eventhub_namespace.eventhub_namespace_dropzone.default_primary_connection_string
    INGRESS_EVENT_HUB_NAME              = azurerm_eventhub.eventhub_dropzone_zipped.name
    INGRESS_EVENT_HUB_CONSUMER_GROUP    = azurerm_eventhub_consumer_group.consumer_group_dropzone_zipped.name

    # Logging Worker
    "Logging__LogLevel__Default"                      = local.LOGGING_LOGLEVEL_WORKER_DEFAULT
    "Logging__LogLevel__Energinet.DataHub.Migrations" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_MIGRATIONS
    "Logging__LogLevel__Energinet.DataHub.Core"       = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE

    # Logging Host
    "AzureFunctionsJobHost__Logging__LogLevel__Default" = local.LOGGING_LOGLEVEL_HOST_DEFAULT
  }

  # Role assigments is needed to connect to the storage accounts using URI
  role_assignments = [
    {
      resource_id          = module.st_dh2data.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      resource_id          = module.st_dh2dropzone.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      resource_id          = module.st_dh2dropzone_archive.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      resource_id          = azurerm_eventhub_namespace.eventhub_namespace_dropzone.id
      role_definition_name = "Azure Event Hubs Data Receiver"
    },
    {
      resource_id          = azurerm_eventhub_namespace.eventhub_namespace_dropzone.id
      role_definition_name = "Azure Event Hubs Data Sender"
    }
  ]
}
