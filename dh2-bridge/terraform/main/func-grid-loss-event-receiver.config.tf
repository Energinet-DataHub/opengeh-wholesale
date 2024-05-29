locals {
  entrypoint_grid_loss_event_receiver = {
    app_settings = {
      "BlobStorageSettings:AccountUri"                        = local.DH2_BRIDGE_DOCUMENT_STORAGE_ACCOUNT_URI
      "BlobStorageSettings:ContainerName"                     = local.DH2_BRIDGE_DOCUMENT_STORAGE_CONTAINER_NAME
      "DatabaseSettings:ConnectionString"                     = local.MS_DH2_BRIDGE_CONNECTION_STRING
      "ConsumeServiceBusSettings:ConnectionString"            = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-listen-connection-string)"
      "ConsumeServiceBusSettings:HealthCheckConnectionString" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-transceiver-connection-string)"
      "ConsumeServiceBusSettings:SharedIntegrationEventTopic" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
      "ConsumeServiceBusSettings:GridLossSubscription"        = module.sbtsub_dh2_bridge_event_listener.name
      "ConverterSettings:RecipientPartyGLN"                   = var.dh2_bridge_recipient_party_gln
      "ConverterSettings:SenderPartyGLN"                      = var.dh2_bridge_sender_party_gln
    }
  }
}
