/*
setting up Databricks jobs for monitoring

adding a sql warehouse
adding a query using the sql warehouse
adding a job to schedule the query
*/

resource "databricks_sql_endpoint" "this" {
  name             = "Migration endpoint"
  cluster_size     = "Small"
  max_num_clusters = 1
  auto_stop_mins   = 60
  warehouse_type   = "PRO"
}

# query for monitor_wholesale_time_series
resource "databricks_sql_query" "monitor_wholesale_time_series" {
  data_source_id = databricks_sql_endpoint.this.data_source_id
  name           = "Monitor_wholesale_time_series"
  query          = <<EOT
    select case when count(*) = 0 then raise_error("table is empty") end from wholesale.time_series_points;
  EOT
}

# query for monitor_wholesale_metering_points
resource "databricks_sql_query" "monitor_wholesale_metering_points" {
  data_source_id = databricks_sql_endpoint.this.data_source_id
  name           = "Monitor_wholesale_metering_point"
  query          = <<EOT
    select case when count(*) = 0 then raise_error("table is empty") end from wholesale.metering_point_periods;
  EOT
}

# query for monitor_eloverblik_time_series
resource "databricks_sql_query" "monitor_eloverblik_time_series" {
  data_source_id = databricks_sql_endpoint.this.data_source_id
  name           = "Monitor_eloverblik_time_series"
  query          = <<EOT
    select case when count(*) = 0 then raise_error("table is empty") end from eloverblik.eloverblik_time_series_points
  EOT
}

# job for monitor_wholesale_time_series
resource "databricks_job" "monitor_wholesale_time_series" {
  name = "Monitor_wholesale_time_series"
  max_concurrent_runs = 2

  schedule {
    quartz_cron_expression = local.monitor_trigger_cron
    timezone_id            = "UTC"
  }

  task {
    task_key = "monitor_wholesale_time_series"

    sql_task {
      warehouse_id = databricks_sql_endpoint.this.id
      query {
        query_id = databricks_sql_query.monitor_wholesale_time_series.id
      }
    }
  }
}


# job for monitor_wholesale_metering_points
resource "databricks_job" "monitor_wholesale_metering_points" {
  name = "Monitor_wholesale_metering_points"
  max_concurrent_runs = 2

  schedule {
    quartz_cron_expression = local.monitor_trigger_cron
    timezone_id            = "UTC"
  }

  task {
    task_key = "monitor_wholesale_metering_points"

    sql_task {
      warehouse_id = databricks_sql_endpoint.this.id
      query {
        query_id = databricks_sql_query.monitor_wholesale_metering_points.id
      }
    }
  }
}

# job for monitor_eloverblik_time_series
resource "databricks_job" "monitor_eloverblik_time_series" {
  name = "Monitor_eloverblik_time_series"
  max_concurrent_runs = 2

  schedule {
    quartz_cron_expression = local.monitor_trigger_cron
    timezone_id            = "UTC"
  }

  task {
    task_key = "monitor_eloverblik_time_series"

    sql_task {
      warehouse_id = databricks_sql_endpoint.this.id
      query {
        query_id = databricks_sql_query.monitor_eloverblik_time_series.id
      }
    }
  }
}
