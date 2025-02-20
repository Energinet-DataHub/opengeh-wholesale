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

module "kvs_databricks_core_warehouse_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "core-warehouse-id"
  value        = databricks_sql_endpoint.core_sql_endpoint.id
  key_vault_id = module.kv_internal.id
}

resource "databricks_sql_endpoint" "measurements_api_sql_endpoint" {
  provider                  = databricks.dbw
  name                      = "Measurements Api SQL endpoint"
  cluster_size              = "Small"
  max_num_clusters          = 1
  auto_stop_mins            = 30
  warehouse_type            = "PRO"
  enable_serverless_compute = false
}

module "kvs_databricks_measurements_api_sql_endpoint_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "dbw-databricks-measurements-api-sql-endpoint-id"
  value        = resource.databricks_sql_endpoint.measurements_api_sql_endpoint.id
  key_vault_id = module.kv_internal.id
}

resource "databricks_permissions" "measurements_api_endpoint_permissions" {
  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.measurements_api_sql_endpoint.id

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
