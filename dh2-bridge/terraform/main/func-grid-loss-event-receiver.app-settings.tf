locals {
  default_event_receiver_app_settings = {
    "BlobStorageSettings:AccountUri"                        = local.DH2_BRIDGE_DOCUMENT_STORAGE_ACCOUNT_URI
    "BlobStorageSettings:ContainerName"                     = local.DH2_BRIDGE_DOCUMENT_STORAGE_CONTAINER_NAME
    "DatabaseSettings:ConnectionString"                     = local.MS_DH2_BRIDGE_CONNECTION_STRING
    "ConsumeServiceBusSettings:ConnectionString"            = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-listen-connection-string)"
    "ConsumeServiceBusSettings:HealthCheckConnectionString" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-transceiver-connection-string)"
    "ConsumeServiceBusSettings:SharedIntegrationEventTopic" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
    "ConsumeServiceBusSettings:GridLossSubscription"        = module.sbtsub_dh2_bridge_event_listener.name
    "ConverterSettings:RecipientPartyGLN"                   = local.DH2_BRIDGE_RECIPIENT_PARTY_GLN
    "ConverterSettings:SenderPartyGLN"                      = local.DH2_BRIDGE_SENDER_PARTY_GLN
  }
}
