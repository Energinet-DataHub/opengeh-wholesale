locals {
  func_notifications_worker = {
    app_settings = {
      "Database:ConnectionString"                        = local.MS_NOTIFICATIONS_CONNECTION_STRING

      # Integration Events
      "IntegrationEvents:TopicName"                      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
      "IntegrationEvents:SubscriptionName"               = module.sbtsub_notifications_event_listener.name
      "ServiceBus:FullyQualifiedNamespace"               = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-namespace-endpoint)"

      # Authentication
      "UserAuthentication__MitIdExternalMetadataAddress" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=mitid-frontend-open-id-url)"
      "UserAuthentication__ExternalMetadataAddress"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=frontend-open-id-url)"
      "UserAuthentication__InternalMetadataAddress"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=api-backend-open-id-url)"
      "UserAuthentication__BackendBffAppId"              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-bff-app-id)"

      # Revision Log
      "RevisionLogOptions__ApiAddress" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-log-ingestion-api-url)"
    }
  }
}
