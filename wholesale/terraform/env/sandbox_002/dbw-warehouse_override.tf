# SQL warehouse to host Databricks SQL Statement Execution API
resource "databricks_sql_endpoint" "this" {
  provider = databricks.dbw
}
