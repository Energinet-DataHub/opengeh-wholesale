resource "databricks_sql_endpoint" "electricity_market_sql_endpoint" {
  provider                  = databricks.dbw
  name                      = "Electricity Market SQL Endpoint"
  cluster_size              = "Small"
  max_num_clusters          = 1
  auto_stop_mins            = 30
  warehouse_type            = "PRO"
  enable_serverless_compute = false
}

resource "databricks_permissions" "electricity_market_sql_endpoint_permissions" {
  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.electricity_market_sql_endpoint.id

  access_control {
    group_name       = var.databricks_contributor_dataplane_group.name
    permission_level = "CAN_MANAGE"
  }
  dynamic "access_control" {
    for_each = local.readers
    content {
      group_name       = access_control.key
      permission_level = "CAN_MONITOR"
    }
  }
  depends_on = [module.dbw]
}

module "kvs_databricks_electricity_market_warehouse_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "electricity-market-warehouse-id"
  value        = databricks_sql_endpoint.electricity_market_sql_endpoint.id
  key_vault_id = module.kv_internal.id
}
