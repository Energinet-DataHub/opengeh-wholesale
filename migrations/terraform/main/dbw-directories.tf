resource "databricks_directory" "shared_alerts_dir" {
  provider = databricks.dbw
  path     = "/Shared/Alerts"
}

resource "databricks_directory" "shared_queries_dir" {
  provider = databricks.dbw
  path     = "/Shared/Queries"
}

resource "databricks_directory" "shared_dashboards_dir" {
  provider = databricks.dbw
  path     = "/Shared/Dashboards"
}
