resource "databricks_sql_endpoint" "migration_sql_endpoint" {
  max_num_clusters = 2
}

resource "databricks_sql_endpoint" "ts_api_sql_endpoint" {
  max_num_clusters = 2
  auto_stop_mins   = 60
}
