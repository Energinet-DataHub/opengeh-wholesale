# SQL warehouse to host Databricks Alerts and Dashboard queries
resource "databricks_sql_endpoint" "migration_sql_endpoint" {
  provider                  = databricks.dbw
  name                      = "Migration SQL endpoint"
  cluster_size              = "Small"
  max_num_clusters          = 1
  auto_stop_mins            = 60
  warehouse_type            = "PRO"
  enable_serverless_compute = false
}

module "kvs_databricks_sql_endpoint_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "dbw-databricks-sql-endpoint-id"
  value        = resource.databricks_sql_endpoint.migration_sql_endpoint.id
  key_vault_id = module.kv_internal.id
}

resource "databricks_permissions" "endpoint_permissions" {
  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.migration_sql_endpoint.id

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

resource "databricks_sql_endpoint" "backup_warehouse" {
  for_each             = local.backup_warehouse_set
  provider             = databricks.dbw
  name                 = "SQL Endpoint for running Deep Clone backups"
  cluster_size         = "Small"
  max_num_clusters     = 2
  auto_stop_mins       = 15
  warehouse_type       = "PRO"
  spot_instance_policy = "RELIABILITY_OPTIMIZED"

  depends_on = [module.dbw]
}

resource "databricks_permissions" "backup_endpoint" {
  for_each        = local.backup_warehouse_set
  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.backup_warehouse[each.key].id

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
