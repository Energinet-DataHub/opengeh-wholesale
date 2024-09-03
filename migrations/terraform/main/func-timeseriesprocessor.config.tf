locals {
  func_timeseriesprocessor = {
    app_settings = {
      WEBSITE_LOAD_CERTIFICATES                                              = local.datahub2_certificate_thumbprint
      "StorageAccount__Dh2TimeSeriesIntermediaryStorageAccountUri"           = "https://${module.st_dh2timeseries_intermediary.name}.blob.core.windows.net"
      "StorageAccount__Dh2TimeSeriesIntermediaryContainerName"               = azurerm_storage_container.timeseriesintermediary.name
      "ServiceBus__ConnectionString"                                         = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-listen-connection-string)"
      "ServiceBus__TimeSeriesMessagesTopicName"                              = azurerm_servicebus_topic.time_series_imported_messages_topic.name
      "ServiceBus__TimeSeriesProcessingSubscriptionName"                     = module.sbtsub_time_series_sync_processing.name
      "FeatureManagement__DataHub2HealthCheck"                               = var.feature_flag_datahub2_healthcheck
      "FeatureManagement__DataHub2TimeSeriesImport"                          = var.feature_flag_datahub2_time_series_import
      "FeatureManagement__PurgeDurableFunctionHistory"                       = var.feature_flag_purge_durable_function_history
      "FeatureManagement__NewRetrieverAndProcessorOrchestration"             = var.feature_flag_new_ts_orchestration
      "TimeSeriesSynchronizationTaskHubName"                                 = "TimeSeriesProcessor01"

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
