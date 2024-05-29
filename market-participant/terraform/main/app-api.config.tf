locals {
  app_api = {
    app_settings = {
      "UserAuthentication:MitIdExternalMetadataAddress" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=mitid-frontend-open-id-url)"
      "UserAuthentication:ExternalMetadataAddress"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=frontend-open-id-url)"
      "UserAuthentication:InternalMetadataAddress"      = "https://app-api-${var.domain_name_short}-${var.environment_short}-we-${var.environment_instance}.azurewebsites.net/.well-known/openid-configuration"
      "UserAuthentication:BackendBffAppId"              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-bff-app-id)"

      "Database:ConnectionString" = local.MS_MARKET_PARTICIPANT_CONNECTION_STRING

      "AzureB2c:Tenant"             = var.b2c_tenant
      "AzureB2c:SpnId"              = var.b2c_spn_id
      "AzureB2c:SpnSecret"          = var.b2c_spn_secret
      "AzureB2c:BackendObjectId"    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-obj-id)"
      "AzureB2c:BackendSpnObjectId" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-sp-id)"
      "AzureB2c:BackendId"          = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-id)"

      "KeyVault:TokenSignKeyVault"    = module.kv_internal.vault_uri
      "KeyVault:TokenSignKeyName"     = azurerm_key_vault_key.token_sign.name
      "KeyVault:CertificatesKeyVault" = module.kv_dh2_certificates.vault_uri

      "CvrRegister:BaseAddress" = var.cvr_base_address
      "CvrRegister:Username"    = var.cvr_username
      "CvrRegister:Password"    = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_cvr_password.name})"
    }
  }
}
