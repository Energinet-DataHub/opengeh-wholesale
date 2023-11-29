resource "databricks_directory" "shared_alerts_dir" {
  provider = databricks.dbw
}

resource "databricks_directory" "shared_queries_dir" {
  provider = databricks.dbw
}

resource "databricks_directory" "shared_dashboards_dir" {
  provider = databricks.dbw
}
