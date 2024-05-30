locals {
  func_entrypoint_exchange_event_receiver = {
    app_settings = {
      "DatabaseSettings:ConnectionString"                     = local.MS_ESETT_EXCHANGE_CONNECTION_STRING
      "ConsumeServiceBusSettings:ConnectionString"            = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-listen-connection-string)"
      "ConsumeServiceBusSettings:HealthCheckConnectionString" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-transceiver-connection-string)"
      "ConsumeServiceBusSettings:SharedIntegrationEventTopic" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
      "ConsumeServiceBusSettings:EsettExchangeSubscription"   = module.sbtsub_esett_exchange_event_listener.name
      "BlobStorageSettings:AccountUri"                        = local.ESETT_DOCUMENT_STORAGE_ACCOUNT_URI
      "BlobStorageSettings:ContainerName"                     = local.ESETT_DOCUMENT_STORAGE_CONTAINER_NAME
    }
  }
}
