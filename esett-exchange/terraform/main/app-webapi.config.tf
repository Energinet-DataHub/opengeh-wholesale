locals {
  app_webapi = {
    app_settings = {
      "JwtBearerSettings:MitIdExternalOpenIdUrl"        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=mitid-frontend-open-id-url)"
      "JwtBearerSettings:ExternalOpenIdUrl"             = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=frontend-open-id-url)"
      "JwtBearerSettings:InternalOpenIdUrl"             = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=api-backend-open-id-url)"
      "JwtBearerSettings:BackendBffAppId"               = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-bff-app-id)"
      "DatabaseSettings:ConnectionString"               = local.MS_ESETT_EXCHANGE_CONNECTION_STRING
      "BlobStorageSettings:AccountUri"                  = local.ESETT_DOCUMENT_STORAGE_ACCOUNT_URI
      "BlobStorageSettings:ContainerName"               = local.ESETT_DOCUMENT_STORAGE_CONTAINER_NAME
      "StatusSettings:Dh2Uri"                           = module.func_entrypoint_peek.default_hostname
      "StatusSettings:ExchangeUri"                      = module.func_entrypoint_exchange_event_receiver.default_hostname
      "StatusSettings:IncomingUri"                      = module.func_entrypoint_ecp_inbox.default_hostname
      "StatusSettings:OutgoingUri"                      = module.func_entrypoint_ecp_outbox.default_hostname
      "UserAuthentication:MitIdExternalMetadataAddress" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=mitid-frontend-open-id-url)"
      "UserAuthentication:ExternalMetadataAddress"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=frontend-open-id-url)"
      "UserAuthentication:InternalMetadataAddress"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=api-backend-open-id-url)"
      "UserAuthentication:BackendBffAppId"              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-bff-app-id)"
    }
  }
}
