locals {
  entrypoint_grid_loss_simulator = {
    app_settings = {
      "ConsumeServiceBusSettings:HealthCheckConnectionString" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-transceiver-connection-string)"
      "ConsumeServiceBusSettings:GridLossSubscription"        = module.sbtsub_dh2_bridge_event_listener.name
      "PublisherOptions:ServiceBusConnectionString"           = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-send-connection-string)"
      "PublisherOptions:TopicName"                            = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
    }
  }
}
