# SQL warehouse to host Databricks Alerts and Dashboard queries
resource "databricks_sql_endpoint" "migration_sql_endpoint" {
  name             = "Migration SQL endpoint"
  cluster_size     = "Small"
  max_num_clusters = 1
  auto_stop_mins   = 10
  warehouse_type   = "PRO"
  enable_serverless_compute = true
}
