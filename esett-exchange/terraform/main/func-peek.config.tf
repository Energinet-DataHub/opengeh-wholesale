locals {
  func_entrypoint_peek = {
    app_settings = {
      "DataHub2Settings:DataHub2Endpoint"                     = "https://${module.app_dh2_placeholder.default_hostname}"

      "IntegrationEvents:TopicName"                           = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
      "IntegrationEvents:SubscriptionName"                    = module.sbtsub_esett_exchange_event_listener.name
      "ServiceBus:FullyQualifiedNamespace"                    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-namespace-endpoint)"

      "BlobStorageSettings:AccountUri"                        = local.ESETT_DOCUMENT_STORAGE_ACCOUNT_URI
      "BlobStorageSettings:ContainerName"                     = local.ESETT_DOCUMENT_STORAGE_CONTAINER_NAME
      WEBSITE_LOAD_CERTIFICATES                               = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=cert-esett-dh2-thumbprint)"
    }
  }
}
