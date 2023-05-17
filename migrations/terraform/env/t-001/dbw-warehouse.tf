# SQL warehouse to host Databricks Alerts and Dashboard queries
resource "databricks_sql_endpoint" "this" {
  name             = "Migration SQL endpoint"
  cluster_size     = "Small"
  max_num_clusters = 1
  auto_stop_mins   = 10
  enable_serverless_compute = true
  warehouse_type   = "PRO"
}
