# SQL warehouse to host Databricks Alerts and Dashboard queries
resource "databricks_sql_endpoint" "migration_sql_endpoint" {
  provider                  = databricks.dbw
  name                      = "Migration SQL endpoint"
  cluster_size              = "Small"
  max_num_clusters          = 1
  auto_stop_mins            = 10
  warehouse_type            = "PRO"
  enable_serverless_compute = false
}

module "kvs_databricks_sql_endpoint_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "dbw-databricks-sql-endpoint-id"
  value        = resource.databricks_sql_endpoint.migration_sql_endpoint.id
  key_vault_id = module.kv_internal.id
}

# TOOD: delete this when we have the new omada group
resource "databricks_permissions" "endpoint_permissions" {
  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.migration_sql_endpoint.id

  access_control {
    group_name       = "SEC-A-GreenForce-DevelopmentTeamAzure"
    permission_level = "CAN_MANAGE"
  }
}

resource "databricks_permissions" "endpoint_permissions_developers" {
  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.migration_sql_endpoint.id

  access_control {
    group_name       = "SEC-G-Datahub-DevelopersAzure"
    permission_level = "CAN_MANAGE"
  }

  depends_on = [module.dbw, null_resource.scim_developers]
}
