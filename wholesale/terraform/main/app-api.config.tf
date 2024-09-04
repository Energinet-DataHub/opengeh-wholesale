locals {
  app_api = {
    app_settings = {
      # Authentication
      "UserAuthentication__MitIdExternalMetadataAddress" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=mitid-frontend-open-id-url)"
      "UserAuthentication__ExternalMetadataAddress"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=frontend-open-id-url)"
      "UserAuthentication__InternalMetadataAddress"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=api-backend-open-id-url)"
      "UserAuthentication__BackendBffAppId"              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-bff-app-id)"

      # Logging
      "Logging__ApplicationInsights__LogLevel__Default"                     = local.LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Wholesale" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_WHOLESALE
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core"      = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE

      # Storage (DataLake)
      STORAGE_CONTAINER_NAME = local.STORAGE_CONTAINER_NAME
      STORAGE_ACCOUNT_URI    = local.STORAGE_ACCOUNT_URI

      # Service Bus
      # TODO: remove this when subsystem has been deployed once with RBAC
      "ServiceBus__ConnectionString"        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-transceiver-connection-string)"
      "ServiceBus__FullyQualifiedNamespace" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-namespace-endpoint)"
      "IntegrationEvents__TopicName"        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
      "IntegrationEvents__SubscriptionName" = module.sbtsub_wholesale_integration_event_listener.name

      # Databricks
      WorkspaceToken        = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-workspace-token)"
      WorkspaceUrl          = "https://${module.dbw.workspace_url}"
      WarehouseId           = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-databricks-sql-endpoint-id)"
      TimeoutInSeconds      = "50" # This corresponds to a total timeout of 500 seconds, because the Databricks module currently is hard coded with 10 retries.
      DatabricksCatalogName = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=shared-unity-catalog-name)"

    }
    connection_strings = [
      {
        name  = "DB_CONNECTION_STRING"
        type  = "SQLAzure"
        value = local.DB_CONNECTION_STRING
      }
    ]
  }
}
