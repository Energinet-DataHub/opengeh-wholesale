locals {
  app_api = {
    app_settings = {
      "Database:ConnectionString" = local.MS_ELECTRICITY_MARKET_CONNECTION_STRING

      "RevisionLogOptions:ApiAddress" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=func-log-ingestion-api-url)"
    }
  }
}
