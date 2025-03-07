module "func_mp_import_df" {
  app_settings = merge(local.func_mp_import_df.app_settings, {
    "Databricks:WorkspaceUrl"   = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-workspace-url)"
    "Databricks:WorkspaceToken" = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-workspace-token)"
    "Databricks:WarehouseId"    = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=electricity-market-warehouse-id)"
  })
}
