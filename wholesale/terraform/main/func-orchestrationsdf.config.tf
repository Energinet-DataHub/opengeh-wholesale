
locals {
  func_orchestrationsdf = {
    app_settings = {
      # Authentication
      "UserAuthentication__MitIdExternalMetadataAddress" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=mitid-frontend-open-id-url)"
      "UserAuthentication__ExternalMetadataAddress"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=frontend-open-id-url)"
      "UserAuthentication__InternalMetadataAddress"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=api-backend-open-id-url)"
      "UserAuthentication__BackendBffAppId"              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-bff-app-id)"

      # Logging
      # => Azure Function Worker
      # Explicit override the default "Warning" level filter set by the Application Insights SDK.
      # Otherwise custom logging at "Information" level is not possible, even if we configure it on categories.
      # For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
      "Logging__ApplicationInsights__LogLevel__Default"                     = "Information"
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Wholesale" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_WHOLESALE
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core"      = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE

      # Storage (DataLake)
      STORAGE_CONTAINER_NAME = local.STORAGE_CONTAINER_NAME
      STORAGE_ACCOUNT_URI    = local.STORAGE_ACCOUNT_URI

      # Service Bus
      "ServiceBus__FullyQualifiedNamespace" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-namespace-endpoint)"
      "IntegrationEvents__TopicName"        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
      "IntegrationEvents__SubscriptionName" = module.sbtsub_wholesale_integration_event_listener.name
      "WholesaleInbox__QueueName"           = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbq-wholesale-inbox-messagequeue-name)"
      "EdiInbox__QueueName"                 = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbq-edi-inbox-messagequeue-name)"

      # Databricks
      WorkspaceToken        = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-workspace-token)"
      WorkspaceUrl          = "https://${module.dbw.workspace_url}"
      WarehouseId           = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-databricks-sql-endpoint-id)"
      DatabricksCatalogName = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=shared-unity-catalog-name)"

      # Database
      "CONNECTIONSTRINGS__DB_CONNECTION_STRING" = local.DB_CONNECTION_STRING

      # Dead-letter logs
      DeadLetterLogging__StorageAccountUrl = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=st-deadltr-shres-blob-url)"
      DeadLetterLogging__ContainerName     = "wholesale-orchestrations"

      # Durable Functions
      # => Task Hub Name
      # See naming constraints: https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-task-hubs?tabs=csharp#task-hub-names
      "OrchestrationsTaskHubName" = "Wholesale01"
      # => Task Hub Storage account connection string
      "OrchestrationsStorageConnectionString" = module.taskhub_storage_account.primary_connection_string

      # Audit Log
      RevisionLogOptions__ApiAddress = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-log-ingestion-api-url)"
    }
  }
}
