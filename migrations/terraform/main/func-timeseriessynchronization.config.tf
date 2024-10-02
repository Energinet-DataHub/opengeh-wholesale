locals {
  func_timeseriessynchronization = {
    app_settings = {
      WEBSITE_LOAD_CERTIFICATES                                              = local.datahub2_certificate_thumbprint
      "AzureWebJobs.ProcessMessageOrchestrationTrigger.Disabled"             = true

      "StorageAccount__Dh2StorageAccountUri"                                 = "https://${module.st_dh2data.name}.blob.core.windows.net"
      "StorageAccount__TimeSeriesContainerName"                              = azurerm_storage_container.dh2_timeseries_synchronization.name # Kept for backwards compatibility
      "StorageAccount__Dh2TimeSeriesSynchronizationContainerName"            = azurerm_storage_container.dh2_timeseries_synchronization.name
      "StorageAccount__Dh2TimeSeriesSynchronizationArchiveStorageAccountUri" = "https://${module.st_dh2dropzone_archive.name}.blob.core.windows.net"
      "StorageAccount__Dh2TimeSeriesSynchronizationArchiveContainerName"     = azurerm_storage_container.dropzonetimeseriessyncarchive.name
      "StorageAccount__Dh2TimeSeriesIntermediaryStorageAccountUri"           = "https://${module.st_dh2timeseries_intermediary.name}.blob.core.windows.net"
      "StorageAccount__Dh2TimeSeriesIntermediaryContainerName"               = azurerm_storage_container.timeseriesintermediary.name
      "StorageAccount__Dh2TimeSeriesAuditDataStorageAccountUri"              = "https://${module.st_dh2timeseries_audit.name}.blob.core.windows.net"
      "StorageAccount__Dh2TimeSeriesAuditDataContainerName"                  = azurerm_storage_container.timeseriesaudit.name
      # TODO: remove "ServiceBus__ConnectionString" when subsystem has been deployed once with RBAC
      "ServiceBus__ConnectionString"                                         = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-manage-connection-string)"
      "ServiceBus__FullyQualifiedNamespace"                                  = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-namespace-endpoint)"
      "ServiceBus__TimeSeriesMessagesTopicName"                              = azurerm_servicebus_topic.time_series_imported_messages_topic.name
      "ServiceBus__TimeSeriesProcessingSubscriptionName"                     = module.sbtsub_time_series_sync_processing.name
      "ServiceBus__TimeSeriesAuditSubscriptionName"                          = module.sbtsub_time_series_sync_audit.name
      "DataHub2Client__EndpointAddress"                                      = var.datahub2_migration_url,
      "FeatureManagement__DataHub2HealthCheck"                               = var.feature_flag_datahub2_healthcheck
      "FeatureManagement__DataHub2TimeSeriesImport"                          = var.feature_flag_datahub2_time_series_import
      "FeatureManagement__PurgeDurableFunctionHistory"                       = var.feature_flag_purge_durable_function_history
      "TimeSeriesSynchronizationTaskHubName"                                 = "TimeSeriesSynchronization01"

      # Logging Worker
      "Logging__ApplicationInsights__LogLevel__Default"                      = local.LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Migrations" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_MIGRATIONS
      "Logging__ApplicationInsights__LogLevel__Energinet.Datahub.Core"       = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE

      # Logging Host
      "AzureFunctionsJobHost__logging__logLevel__Default"                                    = local.AZUREFUNCTIONSJOBHOST_LOGGING_LOGLEVEL_DEFAULT
      "AzureFunctionsJobHost__logging__logLevel__DurableTask.Core"                           = local.AZUREFUNCTIONSJOBHOST_LOGGING_LOGLEVEL_DURABLETASK_CORE
      "AzureFunctionsJobHost__logging__logLevel__DurableTask.AzureStorage"                   = local.AZUREFUNCTIONSJOBHOST_LOGGING_LOGLEVEL_DURABLETASK_AZURESTORAGE
      "AzureFunctionsJobHost__logging__logLevel__Host.Triggers.DurableTask"                  = local.AZUREFUNCTIONSJOBHOST_LOGGING_LOGLEVEL_HOST_TRIGGERS_DURABLETASK
      "AzureFunctionsJobHost__logging__applicationInsights__samplingSettings__isEnabled"     = local.AZUREFUNCTIONSJOBHOST_LOGGING_APPINSIGHTS_SAMPLINGSETTINGS_ISENABLED
      "AzureFunctionsJobHost__logging__applicationInsights__samplingSettings__excludedTypes" = local.AZUREFUNCTIONSJOBHOST_LOGGING_APPINSIGHTS_SAMPLINGSETTINGS_EXCLUDEDTYPES
    }
  }
}
