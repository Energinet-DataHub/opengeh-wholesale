resource "databricks_secret_scope" "migration_scope" {
  provider = databricks.dbw
}

resource "databricks_secret" "spn_app_id" {
  provider = databricks.dbw
}

resource "databricks_secret" "spn_app_secret" {
  provider = databricks.dbw
}

resource "databricks_secret" "appi_instrumentation_key" {
  provider = databricks.dbw
}

resource "databricks_secret" "st_dh2data_storage_account" {
  provider = databricks.dbw
}

resource "databricks_secret" "st_shared_datalake_account" {
  provider = databricks.dbw
}

resource "databricks_secret" "st_migration_datalake_account" {
  provider = databricks.dbw
}
