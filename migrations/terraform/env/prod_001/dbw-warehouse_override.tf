resource "databricks_sql_endpoint" "migration_sql_endpoint" {
  cluster_size = "Medium"
}

resource "databricks_sql_endpoint" "backup_warehouse" {
  cluster_size = "Medium"
}
