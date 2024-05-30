locals {
  func_entrypoint_application_workers = {
    app_settings = {
      "DatabaseSettings:ConnectionString"                     = local.MS_ESETT_EXCHANGE_CONNECTION_STRING
      "PublishServiceBusSettings:ConnectionString"            = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-send-connection-string)"
      "PublishServiceBusSettings:HealthCheckConnectionString" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-transceiver-connection-string)"
      "PublishServiceBusSettings:SharedIntegrationEventTopic" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
      "ConsumeServiceBusSettings:EsettExchangeSubscription"   = module.sbtsub_esett_exchange_event_listener.name
    }
  }
}
