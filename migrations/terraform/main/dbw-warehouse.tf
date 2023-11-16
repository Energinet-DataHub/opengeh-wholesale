# SQL warehouse to host Databricks Alerts and Dashboard queries
resource "databricks_sql_endpoint" "migration_sql_endpoint" {
  name             = "Migration SQL endpoint"
  cluster_size     = "Small"
  max_num_clusters = 1
  auto_stop_mins   = 10
  warehouse_type   = "PRO"
}

module "kvs_databricks_sql_endpoint_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name         = "dbw-databricks-sql-endpoint-id"
  value        = resource.databricks_sql_endpoint.migration_sql_endpoint.id
  key_vault_id = module.kv_internal.id
}
