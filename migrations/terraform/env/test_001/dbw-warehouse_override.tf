resource "databricks_sql_endpoint" "backup_warehouse" {
  cluster_size = "Large" # After first deep-clone, this should be changed to "Medium"
}
