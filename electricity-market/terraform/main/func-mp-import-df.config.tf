locals {
  func_mp_import_df = {
    app_settings = {
      FeatureManagement__ActorTestModeEnabled = false

      # Timeout, required for initial import
      "AzureFunctionsJobHost__functionTimeout" = "2:00:00"

      # Databricks
      "Databricks:WorkspaceUrl"   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-migration-workspace-url)"
      "Databricks:WorkspaceToken" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-migration-workspace-token)"
      "Databricks:WarehouseId"    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-migration-warehouse-id)"

      # Database
      "Database:ConnectionString" = local.MS_MARKPART_DB_CONNECTION_STRING
    }
  }
}
