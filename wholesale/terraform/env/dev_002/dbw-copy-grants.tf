resource "databricks_grant" "wholesale_migrations_wholesale_copy" {
  provider          = databricks.dbw
  external_location = databricks_external_location.wholesale_migrations_wholesale.id
  principal         = var.databricks_contributor_dataplane_group.name
  privileges        = ["READ_FILES", "WRITE_FILES"]

  depends_on = [module.dbw]
}

resource "databricks_grant" "wholesale_internal_copy" {
  provider          = databricks.dbw
  external_location = databricks_external_location.internal.id
  principal         = var.databricks_contributor_dataplane_group.name
  privileges        = ["READ_FILES", "WRITE_FILES"]

  depends_on = [module.dbw]
}

resource "azurerm_storage_container" "wholesale_migrations_wholesale_copy" {
  name                  = "wholesale-migrations-wholesale-restore-data"
  storage_account_name  = module.st_data_wholesale.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "wholesale_internal_copy" {
  name                  = "wholesale-internal-restore-data"
  storage_account_name  = module.st_data_wholesale.name
  container_access_type = "private"
}

resource "databricks_external_location" "wholesale_migrations_wholesale_copy" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.wholesale_migrations_wholesale_copy.name}_${module.st_data_wholesale.name}"
  url             = "abfss://${azurerm_storage_container.wholesale_migrations_wholesale_copy.name}@${module.st_data_wholesale.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw]
}

resource "databricks_external_location" "wholesale_internal_copy" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.wholesale_internal_copy.name}_${module.st_data_wholesale.name}"
  url             = "abfss://${azurerm_storage_container.wholesale_internal_copy.name}@${module.st_data_wholesale.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw]
}
