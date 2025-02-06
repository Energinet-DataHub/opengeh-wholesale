resource "databricks_query" "migration_sql_endpoint_keep_alive" {
  provider     = databricks.dbw
  warehouse_id = databricks_sql_endpoint.migration_sql_endpoint.id
  display_name = "migration_sql_endpoint_keep_alive"
  query_text   = "SELECT 42 as value"
  parent_path  = "/Shared/Queries"
}

resource "databricks_job" "migration_sql_endpoint_keep_alive" {
  provider = databricks.dbw
  name     = "migration_sql_endpoint_keep_alive"

  schedule {
    quartz_cron_expression = "0 0 7-16 ? * MON-FRI"
    timezone_id            = "Europe/Copenhagen"
  }

  task {
    task_key = "migration_sql_endpoint_keep_alive"

    sql_task {
      query {
        query_id = databricks_query.migration_sql_endpoint_keep_alive.id
      }
      warehouse_id = databricks_sql_endpoint.migration_sql_endpoint.id
    }
  }
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
    quartz_cron_expression = "0 0 7-16 ? * MON-FRI"
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
