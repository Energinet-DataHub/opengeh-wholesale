#
# SQL warehouse to support wholesale deployment
#

resource "databricks_sql_endpoint" "deployment_warehouse" {
  provider         = databricks.dbw
  name             = "Wholesale Deployment Warehouse"
  cluster_size     = "Small"
  max_num_clusters = 1
  auto_stop_mins   = 120
  warehouse_type   = "PRO"
  # Enable preview as the statement API is currently in public preview
  channel {
    name = "CHANNEL_NAME_PREVIEW"
  }
}

resource "databricks_permissions" "databricks_permissions_deployment_warehouse" {
  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.deployment_warehouse.id

  access_control {
    group_name       = "SEC-G-Datahub-DevelopersAzure"
    permission_level = "CAN_MANAGE"
  }
  depends_on = [module.dbw, null_resource.scim_developers]
}

#
# Places Databricks secrets in internal key vault
# In order to be able to access the SQL endpoint from deployment pipeline
#

module "kvs_databricks_sql_endpoint_id_deployment" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_4.0.1"

  name         = "dbw-databricks-sql-endpoint-id-deployment"
  value        = resource.databricks_sql_endpoint.deployment_warehouse.id
  key_vault_id = module.kv_internal.id
}
