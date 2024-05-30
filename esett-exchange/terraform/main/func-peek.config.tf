locals {
  func_entrypoint_peek = {
    app_settings = {
      "PublishServiceBusSettings:ConnectionString"            = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-send-connection-string)"
      "PublishServiceBusSettings:HealthCheckConnectionString" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-transceiver-connection-string)"
      "PublishServiceBusSettings:SharedIntegrationEventTopic" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
      "ConsumeServiceBusSettings:EsettExchangeSubscription"   = module.sbtsub_esett_exchange_event_listener.name
      "DataHub2Settings:DataHub2Endpoint"                     = var.dh2_endpoint != null ? var.dh2_endpoint : "https://${module.app_dh2_placeholder.default_hostname}"
      "BlobStorageSettings:AccountUri"                        = local.ESETT_DOCUMENT_STORAGE_ACCOUNT_URI
      "BlobStorageSettings:ContainerName"                     = local.ESETT_DOCUMENT_STORAGE_CONTAINER_NAME
      WEBSITE_LOAD_CERTIFICATES                               = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=cert-esett-dh2-thumbprint)"
    }
  }
}
