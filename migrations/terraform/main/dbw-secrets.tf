resource "databricks_secret_scope" "migration_scope" {
  provider = databricks.dbw
  name     = "migration-scope"
}

resource "databricks_secret" "spn_app_id" {
  provider     = databricks.dbw
  key          = "spn_app_id"
  string_value = azuread_application.app_databricks.application_id
  scope        = databricks_secret_scope.migration_scope.id
}

resource "databricks_secret" "spn_app_secret" {
  provider     = databricks.dbw
  key          = "spn_app_secret"
  string_value = azuread_application_password.secret.value
  scope        = databricks_secret_scope.migration_scope.id
}

resource "databricks_secret" "appi_instrumentation_key" {
  provider     = databricks.dbw
  key          = "appi_instrumentation_key"
  string_value = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  scope        = databricks_secret_scope.migration_scope.id
}

resource "databricks_secret" "st_dh2data_storage_account" {
  provider     = databricks.dbw
  key          = "st_dh2data_storage_account"
  string_value = module.st_dh2data.name
  scope        = databricks_secret_scope.migration_scope.id
}

resource "databricks_secret" "st_shared_datalake_account" {
  provider     = databricks.dbw
  key          = "st_shared_datalake_account"
  string_value = data.azurerm_key_vault_secret.st_data_lake_name.value
  scope        = databricks_secret_scope.migration_scope.id
}

resource "databricks_secret" "st_migration_datalake_account" {
  provider     = databricks.dbw
  key          = "st_migration_datalake_account"
  string_value = module.st_migrations.name
  scope        = databricks_secret_scope.migration_scope.id
}
