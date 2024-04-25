resource "random_string" "taskhubname_postfix" {
  length  = 8
  special = false
  numeric = false
  upper   = false
}

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
  dotnet_framework_version                 = "v8.0"
  use_dotnet_isolated_runtime              = true
  durable_function                         = true
  use_32_bit_worker                        = false
  health_check_path                        = "/api/monitor/ready"
  health_check_alert = {
    action_group_id = data.azurerm_key_vault_secret.primary_action_group_id.value
    enabled         = var.enable_health_check_alerts
  }
  role_assignments = [
    {
      resource_id          = module.st_dh2data.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      resource_id          = module.st_dh2dropzone_archive.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      resource_id          = module.st_dh2timeseries_intermediary.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      resource_id          = module.kv_internal.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      resource_id          = module.kv_internal.id
      role_definition_name = "Key Vault Crypto User"
    },
    {
      resource_id          = module.kv_internal.id
      role_definition_name = "Key Vault Secrets Officer"
    },
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    }
  ]

  app_settings = {
    WEBSITE_LOAD_CERTIFICATES                                                    = local.datahub2_certificate_thumbprint
    "TimeSeriesSynchronizationTaskHubName"                                       = "TimeSeriesSynchronization${random_string.taskhubname_postfix.result}"
    "StorageAccount__Dh2StorageAccountUri"                                       = "https://${module.st_dh2data.name}.blob.core.windows.net"
    "StorageAccount__TimeSeriesContainerName"                                    = azurerm_storage_container.dh2_timeseries_synchronization.name # Kept for backwards compatibility
    "StorageAccount__Dh2TimeSeriesSynchronizationContainerName"                  = azurerm_storage_container.dh2_timeseries_synchronization.name
    "StorageAccount__Dh2TimeSeriesSynchronizationArchiveStorageAccountUri"       = "https://${module.st_dh2dropzone_archive.name}.blob.core.windows.net"
    "StorageAccount__Dh2TimeSeriesSynchronizationArchiveContainerName"           = azurerm_storage_container.dropzonetimeseriessyncarchive.name
    "StorageAccount__Dh2TimeSeriesIntermediaryStorageAccountUri"                 = "https://${module.st_dh2timeseries_intermediary.name}.blob.core.windows.net"
    "StorageAccount__Dh2TimeSeriesIntermediaryContainerName"                     = azurerm_storage_container.timeseriesintermediary.name
    "ServiceBus__ConnectionString"                                               = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-transceiver-connection-string)"
    "ServiceBus__TimeSeriesMessagesQueueName"                                    = azurerm_servicebus_queue.time_series_imported_messages_queue.name
    "DataHub2Client__EndpointAddress"                                            = var.datahub2_migration_url,
    "FeatureManagement__DataHub2HealthCheck"                                     = var.feature_flag_datahub2_healthcheck
    "FeatureManagement__DataHub2TimeSeriesImport"                                = var.feature_flag_datahub2_time_series_import

    # Logging Worker
    "Logging__ApplicationInsights__LogLevel__Default"                            = local.LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT
    "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Migrations"       = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_MIGRATIONS
    "Logging__ApplicationInsights__LogLevel__Energinet.Datahub.Core"             = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE
  }
}
