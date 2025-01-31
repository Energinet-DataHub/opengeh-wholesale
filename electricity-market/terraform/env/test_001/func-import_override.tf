module "func_import" {
  app_settings = merge(local.func_import.app_settings, {
    "Databricks:WorkspaceUrl"   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-migration-workspace-url)"
    "Databricks:WorkspaceToken" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-migration-workspace-token)"
    "Databricks:WarehouseId"    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-migration-warehouse-id)"

    "EnableDatabricksHealthCheck" = true
  })
}
