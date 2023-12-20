/*
setting up Databricks jobs for monitoring

adding a sql warehouse
adding a query using the sql warehouse
adding a job to schedule the query
*/

# query for monitor_wholesale_time_series
resource "databricks_sql_query" "monitor_wholesale_time_series" {
  provider       = databricks.dbw
  data_source_id = databricks_sql_endpoint.migration_sql_endpoint.data_source_id
  name           = "Monitor_wholesale_time_series"
  query          = <<EOT
    select case when count(*) = 0 then raise_error("table is empty") end from wholesale.time_series_points;
  EOT
  parent         = "folders/${databricks_directory.shared_queries_dir.object_id}"
}

# query for monitor_wholesale_metering_points
resource "databricks_sql_query" "monitor_wholesale_metering_points" {
  provider       = databricks.dbw
  data_source_id = databricks_sql_endpoint.migration_sql_endpoint.data_source_id
  name           = "Monitor_wholesale_metering_point"
  query          = <<EOT
    select case when count(*) = 0 then raise_error("table is empty") end from wholesale.metering_point_periods;
  EOT
  parent         = "folders/${databricks_directory.shared_queries_dir.object_id}"
}

# query for monitor_eloverblik_time_series
resource "databricks_sql_query" "monitor_eloverblik_time_series" {
  provider       = databricks.dbw
  data_source_id = databricks_sql_endpoint.migration_sql_endpoint.data_source_id
  name           = "Monitor_eloverblik_time_series"
  query          = <<EOT
    select case when count(*) = 0 then raise_error("table is empty") end from eloverblik.eloverblik_time_series_points
  EOT
  parent         = "folders/${databricks_directory.shared_queries_dir.object_id}"
}

# job for monitor_wholesale_time_series
resource "databricks_job" "monitor_wholesale_time_series" {
  provider            = databricks.dbw
  name                = "Monitor_wholesale_time_series"
  max_concurrent_runs = 2

  schedule {
    quartz_cron_expression = local.monitor_trigger_cron
    timezone_id            = "UTC"
  }

  task {
    task_key = "monitor_wholesale_time_series"

    sql_task {
      warehouse_id = databricks_sql_endpoint.migration_sql_endpoint.id
      query {
        query_id = databricks_sql_query.monitor_wholesale_time_series.id
      }
    }
  }
}


# job for monitor_wholesale_metering_points
resource "databricks_job" "monitor_wholesale_metering_points" {
  provider            = databricks.dbw
  name                = "Monitor_wholesale_metering_points"
  max_concurrent_runs = 2

  schedule {
    quartz_cron_expression = local.monitor_trigger_cron
    timezone_id            = "UTC"
  }

  task {
    task_key = "monitor_wholesale_metering_points"

    sql_task {
      warehouse_id = databricks_sql_endpoint.migration_sql_endpoint.id
      query {
        query_id = databricks_sql_query.monitor_wholesale_metering_points.id
      }
    }
  }
}

# job for monitor_eloverblik_time_series
resource "databricks_job" "monitor_eloverblik_time_series" {
  provider            = databricks.dbw
  name                = "Monitor_eloverblik_time_series"
  max_concurrent_runs = 2

  schedule {
    quartz_cron_expression = local.monitor_trigger_cron
    timezone_id            = "UTC"
  }

  task {
    task_key = "monitor_eloverblik_time_series"

    sql_task {
      warehouse_id = databricks_sql_endpoint.migration_sql_endpoint.id
      query {
        query_id = databricks_sql_query.monitor_eloverblik_time_series.id
      }
    }
  }
}
