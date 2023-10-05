# SQL warehouse to host Databricks Alerts and Dashboard queries
resource "databricks_sql_endpoint" "migration_sql_endpoint" {
  enable_serverless_compute = false
}
