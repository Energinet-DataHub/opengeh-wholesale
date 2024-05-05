data "azurerm_key_vault_secret" "unity_storage_credential_id" {
  name         = "unity-storage-credential-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "shared_unity_catalog_name" {
  name         = "shared-unity-catalog-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

resource "databricks_catalog_workspace_binding" "shared" {
  provider = databricks.dbw

  securable_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  workspace_id   = module.dbw.workspace_id

  depends_on = [module.dbw]
}

resource "databricks_external_location" "shared_delta_lake" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.storage_container.name}_${data.azurerm_key_vault_secret.st_data_lake_name.value}"
  url             = "abfss://${azurerm_storage_container.storage_container.name}@${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, databricks_catalog_workspace_binding.shared, data.azurerm_key_vault_secret.st_data_lake_name]
}

resource "databricks_schema" "shared_delta_lake_wholesale" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "wholesale"
  comment      = "Wholesale Schema"
  storage_root = databricks_external_location.shared_delta_lake.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}
