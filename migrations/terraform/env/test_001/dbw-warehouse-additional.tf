# kvs_mig_dbw_warehouse_id is temporary until a separate Databricks workspace is available for the Electricity market

module "kvs_mig_dbw_warehouse_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "dbw-migration-warehouse-id"
  value        = resource.databricks_sql_endpoint.migration_sql_endpoint.id
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

resource "databricks_query" "ts_api_sql_warehouse_keep_alive" {
  provider     = databricks.dbw
  warehouse_id = databricks_sql_endpoint.ts_api_sql_endpoint.id
  display_name = "ts_api_sql_warehouse_keep_alive"
  query_text   = "SELECT 42 as value"
  parent_path  = "/Shared/Queries"
}

resource "databricks_job" "ts_api_sql_warehouse_keep_alive" {
  provider = databricks.dbw
  name     = "ts_api_sql_warehouse_keep_alive"

  schedule {
    quartz_cron_expression = "0 0 6-16 ? * *"
    timezone_id            = "Europe/Copenhagen"
  }

  task {
    task_key = "ts_api_sql_warehouse_keep_alive"

    sql_task {
      query {
        query_id = databricks_query.ts_api_sql_warehouse_keep_alive.id
      }
      warehouse_id = databricks_sql_endpoint.ts_api_sql_endpoint.id
    }
  }
}

removed {
  from = databricks_query.ts_api_sql_endpoint_keep_alive

  lifecycle {
    destroy = false
  }
}
