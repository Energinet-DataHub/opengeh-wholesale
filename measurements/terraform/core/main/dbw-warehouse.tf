resource "databricks_sql_endpoint" "core_sql_endpoint" {
  provider                  = databricks.dbw
  name                      = "Core SQL Endpoint"
  cluster_size              = "Small"
  max_num_clusters          = 1
  auto_stop_mins            = 30
  warehouse_type            = "PRO"
  enable_serverless_compute = false
}

resource "databricks_permissions" "core_sql_endpoint_permissions" {
  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.core_sql_endpoint.id

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
