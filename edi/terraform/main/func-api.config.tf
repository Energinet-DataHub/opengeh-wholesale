locals {
  func_receiver = {
    app_settings = {
      # Shared resources logging
      REQUEST_RESPONSE_LOGGING_CONNECTION_STRING = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=st-marketoplogs-primary-connection-string)",
      REQUEST_RESPONSE_LOGGING_CONTAINER_NAME    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=st-marketoplogs-container-name)",
      B2C_TENANT_ID                              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=b2c-tenant-id)",
      BACKEND_SERVICE_APP_ID                     = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-id)",
      # Endregion: Default Values
      DB_CONNECTION_STRING      = local.CONNECTION_STRING
      AZURE_STORAGE_ACCOUNT_URL = local.AZURE_STORAGE_ACCOUNT_URL

      # File storage
      FileStorage__StorageAccountUrl = local.AZURE_STORAGE_ACCOUNT_URL

      # Logging
      "Logging__ApplicationInsights__LogLevel__Default"                = local.LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Edi"  = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_EDI
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE

      # Audit Log
      RevisionLogOptions__ApiAddress = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-log-ingestion-api-url)"

      # FeatureManagement
      FeatureManagement__UsePeekMessages                                     = var.feature_management_use_peek_messages
      FeatureManagement__ReceiveMeteredDataForMeasurementPoints              = var.feature_management_receive_metered_data_for_measurement_points
      FeatureManagement__UsePeekTimeSeriesMessages                           = var.feature_management_use_peek_time_series_messages
      FeatureManagement__UseRequestWholesaleServicesProcessOrchestration     = var.feature_management_use_request_wholesale_services_process_orchestration
      FeatureManagement__UseRequestAggregatedMeasureDataProcessOrchestration = var.feature_management_use_request_aggregated_measure_data_process_orchestration
      FeatureManagement__UseProcessManagerToEnqueueBrs023027Messages         = var.feature_management_use_process_manager_to_enqueue_brs023027_messages

      # Service Bus
      ServiceBus__FullyQualifiedNamespace = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-namespace-endpoint)"

      # Queue names
      EdiInbox__QueueName         = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbq-edi-inbox-messagequeue-name)"
      WholesaleInbox__QueueName   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbq-wholesale-inbox-messagequeue-name)"
      IncomingMessages__QueueName = azurerm_servicebus_queue.edi_incoming_messages_queue.name

      IntegrationEvents__TopicName        = local.INTEGRATION_EVENTS_TOPIC_NAME
      IntegrationEvents__SubscriptionName = module.sbtsub_edi_integration_event_listener.name

      # Process Manager
      ProcessManagerServiceBusClient__StartTopicName  = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-processmanagerstart-name)"
      ProcessManagerServiceBusClient__NotifyTopicName = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-processmanagernotify-name)"
      ProcessManagerServiceBusClient__Brs021ForwardMeteredDataStartTopicName  = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-brs021forwardmetereddatastart-name)"
      ProcessManagerServiceBusClient__Brs021ForwardMeteredDataNotifyTopicName = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-brs021forwardmetereddatanotify-name)"

      EdiTopic__Name                                                 = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-edi-name)"
      EdiTopic__EnqueueBrs_023_027_SubscriptionName                  = module.sbtsub_edi_enqueue_brs_023_027.name
      EdiTopic__EnqueueBrs_026_SubscriptionName                      = module.sbtsub_edi_enqueue_brs_026.name
      EdiTopic__EnqueueBrs_028_SubscriptionName                      = module.sbtsub_edi_enqueue_brs_028.name
      EdiTopic__EnqueueBrs_021_Forward_Metered_Data_SubscriptionName = module.sbtsub_edi_enqueue_brs_021_forward_metered_data.name

      # Databricks
      WorkspaceToken             = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-wholesale-workspace-token)"
      WorkspaceUrl               = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-wholesale-workspace-url)"
      WarehouseId                = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-wholesale-warehouse-id)"
      EdiDatabricks__CatalogName = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=shared-unity-catalog-name)"
      # DatabricksCatalogName is used in CalculationResults (actor requests copied from Wholesale)
      DatabricksCatalogName = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=shared-unity-catalog-name)"

      # Dead-letter logs
      DeadLetterLogging__StorageAccountUrl = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=st-deadltr-shres-blob-url)"
      DeadLetterLogging__ContainerName     = "edi-b2bapi"

      # Durable Functions
      # => Task Hub Name
      # See naming constraints: https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-task-hubs?tabs=csharp#task-hub-names
      "OrchestrationsTaskHubName"             = "Edi01"
      # => Task Hub Storage account connection string
      "OrchestrationsStorageConnectionString" = module.taskhub_storage_account.primary_connection_string

      # App Configuration Feature Toggling
      AppConfigEndpoint                       = data.azurerm_key_vault_secret.app_configuration_endpoint.value
    }
  }
}
