resource "databricks_secret_scope" "wholesale" {
  provider = databricks.dbw
}

resource "databricks_secret" "spn_app_id" {
  provider = databricks.dbw
}

resource "databricks_secret" "spn_app_secret" {
  provider = databricks.dbw
}
