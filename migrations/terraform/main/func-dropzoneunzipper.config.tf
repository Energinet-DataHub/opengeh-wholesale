locals {
  func_dropzoneunzipper = {
    app_settings = {
      WEBSITE_ENABLE_SYNC_UPDATE_SITE     = true
      WEBSITE_RUN_FROM_PACKAGE            = 1
      WEBSITES_ENABLE_APP_SERVICE_STORAGE = true
      FUNCTIONS_WORKER_RUNTIME            = "dotnet-isolated"

      # Storage Account and container settings - deprecated
      ARCHIVE_DROPZONE_URI                           = "https://${module.st_dh2dropzone_archive.name}.blob.core.windows.net"
      ARCHIVE_CONTAINER_NAME                         = azurerm_storage_container.dropzonearchive.name
      ZIPPED_DROPZONE_URI                            = "https://${module.st_dh2dropzone.name}.blob.core.windows.net"
      ZIPPED_CONTAINER_NAME                          = azurerm_storage_container.dh2_dropzone_zipped.name
      UNZIPPED_DROPZONE_URI                          = "https://${module.st_dh2data.name}.blob.core.windows.net"
      UNZIPPED_METERING_POINTS_CONTAINER_NAME        = azurerm_storage_container.dh2_metering_point_history.name
      UNZIPPED_TIME_SERIES_CONTAINER_NAME            = azurerm_storage_container.dh2_timeseries.name
      UNZIPPED_CHARGES_CONTAINER_NAME                = azurerm_storage_container.dh2_charges.name
      UNZIPPED_CHARGE_LINKS_CONTAINER_NAME           = azurerm_storage_container.dh2_charge_links.name
      UNZIPPED_CONSUMPTION_STATEMENTS_CONTAINER_NAME = azurerm_storage_container.dh2_consumption_statements.name

      # Storage Account settings
      "StorageAccount__DropzoneArchiveUri"                  = "https://${module.st_dh2dropzone_archive.name}.blob.core.windows.net"
      "StorageAccount__DropzoneUri"                         = "https://${module.st_dh2dropzone.name}.blob.core.windows.net"
      "StorageAccount__Dh2DataUri"                          = "https://${module.st_dh2data.name}.blob.core.windows.net"
      "StorageAccount__DropzoneArchiveContainerName"        = azurerm_storage_container.dropzonearchive.name
      "StorageAccount__DropzoneContainerName"               = azurerm_storage_container.dh2_dropzone_zipped.name
      "StorageAccount__MeteringPointsContainerName"         = azurerm_storage_container.dh2_metering_point_history.name
      "StorageAccount__TimeSeriesContainerName"             = azurerm_storage_container.dh2_timeseries.name
      "StorageAccount__ChargesContainerName"                = azurerm_storage_container.dh2_charges.name
      "StorageAccount__ChargeLinksContainerName"            = azurerm_storage_container.dh2_charge_links.name
      "StorageAccount__ConsumptionStatementsContainerName" = azurerm_storage_container.dh2_consumption_statements.name

      # Event Hub settings - deprecated
      INGRESS_EVENT_HUB_CONNECTION_STRING = azurerm_eventhub_namespace.eventhub_namespace_dropzone.default_primary_connection_string
      INGRESS_EVENT_HUB_NAME              = azurerm_eventhub.eventhub_dropzone_zipped.name
      INGRESS_EVENT_HUB_CONSUMER_GROUP    = azurerm_eventhub_consumer_group.consumer_group_dropzone_zipped.name

      # Event Hub settings
      "EventHub__Name"              = azurerm_eventhub.eventhub_dropzone_zipped.name
      "EventHub__ConnectionString"  = azurerm_eventhub_namespace.eventhub_namespace_dropzone.default_primary_connection_string
      "EventHub__ConsumerGroup"     = azurerm_eventhub_consumer_group.consumer_group_dropzone_zipped.name

      # Logging Worker
      "Logging__ApplicationInsights__LogLevel__Default"                      = local.LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Migrations" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_MIGRATIONS
      "Logging__ApplicationInsights__LogLevel__Energinet.Datahub.Core"       = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE

      # Logging Host
      "AzureFunctionsJobHost__logging__logLevel__Default"                                    = local.AZUREFUNCTIONSJOBHOST_LOGGING_LOGLEVEL_DEFAULT
      "AzureFunctionsJobHost__logging__applicationInsights__samplingSettings__isEnabled"     = local.AZUREFUNCTIONSJOBHOST_LOGGING_APPINSIGHTS_SAMPLINGSETTINGS_ISENABLED
      "AzureFunctionsJobHost__logging__applicationInsights__samplingSettings__excludedTypes" = local.AZUREFUNCTIONSJOBHOST_LOGGING_APPINSIGHTS_SAMPLINGSETTINGS_EXCLUDEDTYPES
    }
  }
}
