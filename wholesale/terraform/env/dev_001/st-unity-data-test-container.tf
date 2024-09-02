# The storage containers are not created in the module, as they are used in schema creation. I.e., we want it dynamically
resource "azurerm_storage_container" "wholesale_migrations_wholesale" {
  name                 = "wholesale-migrations-wholesale"
  storage_account_name = module.st_data_wholesale.name
}

resource "databricks_external_location" "wholesale_migrations_wholesale" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.wholesale_migrations_wholesale.name}_${module.st_data_wholesale.name}"
  url             = "abfss://${azurerm_storage_container.wholesale_migrations_wholesale.name}@${module.st_data_wholesale.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, data.azurerm_key_vault_secret.st_data_lake_name]
}

resource "databricks_schema" "wholesale_migrations_wholesale" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "wholesale_migrations_wholesale"
  comment      = "wholesale_migrations_wholesale Schema"
  storage_root = databricks_external_location.wholesale_migrations_wholesale.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token]
}
