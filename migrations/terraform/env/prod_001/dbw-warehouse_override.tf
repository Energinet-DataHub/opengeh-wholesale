resource "databricks_sql_endpoint" "migration_sql_endpoint" {
  cluster_size = "Medium"
}

resource "databricks_sql_endpoint" "investigate_sql_endpoint" {
  cluster_size = "2X-Large"
}

resource "databricks_sql_endpoint" "ts_api_sql_endpoint" {
  max_num_clusters = 4
  auto_stop_mins   = 61
}
