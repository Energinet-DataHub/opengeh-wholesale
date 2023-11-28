# SQL warehouse to host Databricks Alerts and Dashboard queries
resource "databricks_sql_endpoint" "migration_sql_endpoint" {
  provider                  = databricks.dbw
  enable_serverless_compute = false
}
