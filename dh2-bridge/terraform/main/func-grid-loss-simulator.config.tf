locals {
  entrypoint_grid_loss_simulator = {
    app_settings = {
      "IntegrationEvents:TopicName"                           = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
      "IntegrationEvents:SubscriptionName"                    = module.sbtsub_dh2_bridge_event_listener.name
      "ServiceBus:FullyQualifiedNamespace"                    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-namespace-endpoint)"
    }
  }
}
