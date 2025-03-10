locals {
  b2c_web_api = {
    app_settings = {
      DB_CONNECTION_STRING      = local.CONNECTION_STRING
      AZURE_STORAGE_ACCOUNT_URL = local.AZURE_STORAGE_ACCOUNT_URL

      # File storage
      FileStorage__StorageAccountUrl = local.AZURE_STORAGE_ACCOUNT_URL

      # Authentication
      UserAuthentication__ExternalMetadataAddress      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=frontend-open-id-url)"
      UserAuthentication__InternalMetadataAddress      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=api-backend-open-id-url)"
      UserAuthentication__BackendBffAppId              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-bff-app-id)"
      UserAuthentication__MitIdExternalMetadataAddress = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=mitid-frontend-open-id-url)"

      # Service Bus
      ServiceBus__FullyQualifiedNamespace = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-namespace-endpoint)"

      # Queue names
      IncomingMessages__QueueName = azurerm_servicebus_queue.edi_incoming_messages_queue.name

      # Process Manager
      ProcessManagerServiceBusClient__StartTopicName  = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-processmanagerstart-name)"
      ProcessManagerServiceBusClient__NotifyTopicName = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-processmanagernotify-name)"
      ProcessManagerServiceBusClient__Brs021ForwardMeteredDataStartTopicName  = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-brs021forwardmetereddatastart-name)"
      ProcessManagerServiceBusClient__Brs021ForwardMeteredDataNotifyTopicName = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-brs021forwardmetereddatanotify-name)"

      # Logging
      "Logging__ApplicationInsights__LogLevel__Default"                = local.LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Edi"  = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_EDI
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE

      # Durable client (orchestrations)
      OrchestrationsStorageAccountConnectionString = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=func-edi-api-taskhub-storage-connection-string)"
      OrchestrationsTaskHubName                    = local.OrchestrationsTaskHubName

      # Feature flags
      FeatureManagement__UseRequestWholesaleServicesProcessOrchestration     = var.feature_management_use_request_wholesale_services_process_orchestration
      FeatureManagement__UseRequestAggregatedMeasureDataProcessOrchestration = var.feature_management_use_request_aggregated_measure_data_process_orchestration
    }
  }
}
