resource "databricks_directory" "shared_alerts_dir" {
  path = "/Shared/Alerts"
}

resource "databricks_directory" "shared_queries_dir" {
  path = "/Shared/Queries"
}

resource "databricks_directory" "shared_dashboards_dir" {
  path = "/Shared/Dashboards"
}
