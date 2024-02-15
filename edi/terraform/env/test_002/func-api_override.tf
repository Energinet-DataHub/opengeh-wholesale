module "func_receiver" {
  app_settings = {
    # Shared resources logging
    REQUEST_RESPONSE_LOGGING_CONNECTION_STRING = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=st-marketoplogs-primary-connection-string)",
    REQUEST_RESPONSE_LOGGING_CONTAINER_NAME    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=st-marketoplogs-container-name)",
    B2C_TENANT_ID                              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=b2c-tenant-id)",
    BACKEND_SERVICE_APP_ID                     = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-id)",
    # Endregion: Default Values
    DB_CONNECTION_STRING                                    = local.CONNECTION_STRING
    AZURE_STORAGE_ACCOUNT_URL                               = local.AZURE_STORAGE_ACCOUNT_URL
    EDI_INBOX_MESSAGE_QUEUE_NAME                            = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbq-edi-inbox-messagequeue-name)"
    WHOLESALE_INBOX_MESSAGE_QUEUE_NAME                      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbq-wholesale-inbox-messagequeue-name)"
    SERVICE_BUS_CONNECTION_STRING_FOR_DOMAIN_RELAY_LISTENER = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-listen-connection-string)"
    SERVICE_BUS_CONNECTION_STRING_FOR_DOMAIN_RELAY_MANAGE   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-manage-connection-string)"
    SERVICE_BUS_CONNECTION_STRING_FOR_DOMAIN_RELAY_SEND     = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-send-connection-string)"
    INCOMING_MESSAGES_QUEUE_NAME                            = azurerm_servicebus_queue.edi_incoming_messages_queue.name
    INTEGRATION_EVENTS_TOPIC_NAME                           = local.INTEGRATION_EVENTS_TOPIC_NAME
    INTEGRATION_EVENTS_SUBSCRIPTION_NAME                    = module.sbtsub_edi_integration_event_listener.name
    # Feature flags
    FeatureManagement__UseMonthlyAmountPerChargeResultProduced = "true"
    # Endregion: Feature flags
  }
}
