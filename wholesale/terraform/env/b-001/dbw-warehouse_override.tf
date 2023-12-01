# SQL warehouse to host Databricks Alerts and Dashboard queries
resource "databricks_sql_endpoint" "this" {
  enable_serverless_compute = false
}
