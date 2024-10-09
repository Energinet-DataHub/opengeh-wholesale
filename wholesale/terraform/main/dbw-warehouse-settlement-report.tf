# SQL warehouse to host Databricks SQL Statement Execution API
resource "databricks_sql_endpoint" "settlement_report_sql_endpoint" {
  provider             = databricks.dbw
  name                 = "Settlement Report SQL Endpoint"
  cluster_size         = "Medium"
  min_num_clusters     = 1
  max_num_clusters     = 10
  auto_stop_mins       = var.settlement_report_auto_stop_minutes
  warehouse_type       = "PRO"
  spot_instance_policy = "RELIABILITY_OPTIMIZED"
  channel {
    name = "CHANNEL_NAME_CURRENT"
  }
}

resource "databricks_permissions" "databricks_sql_endpoint_settlement_report" {
  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.settlement_report_sql_endpoint.id

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

#
# Places Databricks secrets in shared key vault so settlement report can use data.
#

module "kvs_settlement_report_databricks_warehouse_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_5.0.0"

  name         = "dbw-settlement-report-sql-endpoint-id"
  value        = resource.databricks_sql_endpoint.settlement_report_sql_endpoint.id
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
