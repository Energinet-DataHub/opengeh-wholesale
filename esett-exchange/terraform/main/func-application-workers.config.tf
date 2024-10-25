locals {
  func_entrypoint_application_workers = {
    app_settings = {
      "DatabaseSettings:ConnectionString"                     = local.MS_ESETT_EXCHANGE_CONNECTION_STRING

      "IntegrationEvents:TopicName"                           = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
      "IntegrationEvents:SubscriptionName"                    = module.sbtsub_esett_exchange_event_listener.name
      "ServiceBus:FullyQualifiedNamespace"                    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-namespace-endpoint)"
    }
  }
}
