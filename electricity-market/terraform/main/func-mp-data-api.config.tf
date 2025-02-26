locals {
  func_mp_data_api = {
    app_settings = {
      # Authentication
      "Auth__ApplicationIdUri" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=electricitymarket-application-id-uri)"
      "Auth__Issuer"           = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"

      "Database:ConnectionString" = local.MS_MARKPART_DB_CONNECTION_STRING
    }
  }
}
