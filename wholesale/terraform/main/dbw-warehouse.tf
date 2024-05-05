# SQL warehouse to host Databricks SQL Statement Execution API
resource "databricks_sql_endpoint" "this" {
  provider         = databricks.dbw
  name             = "Wholesale SQL Endpoint"
  cluster_size     = "Small"
  max_num_clusters = 1
  auto_stop_mins   = 120
  warehouse_type   = "PRO"
  # Enable preview as the statement API is currently in public preview
  channel {
    name = "CHANNEL_NAME_PREVIEW"
  }
}

module "kvs_databricks_sql_endpoint_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "dbw-databricks-sql-endpoint-id"
  value        = resource.databricks_sql_endpoint.this.id
  key_vault_id = module.kv_internal.id
}

resource "databricks_permissions" "databricks_sql_endpoint" {
  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.this.id

  access_control { # TOOD: delete this when we have the new omada group
    group_name       = "SEC-A-GreenForce-DevelopmentTeamAzure"
    permission_level = "CAN_MANAGE"
  }

  access_control {
    group_name       = "SEC-G-Datahub-DevelopersAzure"
    permission_level = "CAN_MANAGE"
  }
  depends_on = [module.dbw, null_resource.scim_developers]

}
