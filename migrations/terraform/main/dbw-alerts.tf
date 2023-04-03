/*
setting up Databricks Alerts

adding a sql warehouse
adding a query using the sql warehouse
adding an alert using the query
adding a job to schedule the alert
*/

resource "databricks_sql_endpoint" "this" {
  name = "Migration endpoint"
  cluster_size = "Small"
  max_num_clusters = 1
  auto_stop_mins = 60
  warehouse_type = "PRO"
}

# query for duplicates in silver.time_series
resource "databricks_sql_query" "duplicates_time_series_silver" {
  data_source_id = databricks_sql_endpoint.this.data_source_id
  name = "QCTS30-01_duplicates_in_time_series_silver"
  query =  <<EOT
    select * from
    (select ts.metering_point_id, ts.transaction_id, ts.transaction_insert_date, ts.historical_flag, ts.valid_from_date, ts.valid_to_date, ROW_NUMBER()
    OVER (PARTITION BY ts.metering_point_id, ts.transaction_id, ts.transaction_insert_date, ts.historical_flag, ts.valid_from_date, ts.valid_to_date ORDER BY ts.metering_point_id DESC, ts.valid_from_date DESC) as rownumber
    from silver.time_series as ts) as withRownumber
    where withRownumber.rownumber > 1
  EOT
}

# alert for duplicates in silver.time_series
resource "databricks_sql_alert" "duplicates_time_series_silver" {
  name = "Duplicates found in time series silver"
  query_id = databricks_sql_query.duplicates_time_series_silver.id
  rearm = 1
  options {
    column = "rownumber"
    op = "!="
    value = "0"
    muted = false
  }
}

# job for duplicates in silver.time_series
resource "databricks_job" "duplicates_time_series_silver" {
  name = "Duplicates_time_series_silver"

  schedule {
    quartz_cron_expression = local.alert_trigger_cron
    timezone_id = "UTC"
  }

  email_notifications {
    on_failure = [local.alert_job_email_notification]
  }

  task {
    task_key = "check"

    sql_task {
      warehouse_id = databricks_sql_endpoint.this.id
      alert {
        alert_id = databricks_sql_alert.duplicates_time_series_silver.id
      }
    }
  }
}

# query for duplicates in gold.time_series_points
resource "databricks_sql_query" "duplicates_time_series_gold" {
  data_source_id = databricks_sql_endpoint.this.data_source_id
  name = "QCTS30-01_duplicates_in_time_series_gold"
  query =  <<EOT
    select *
    from (select ts.MeteringPointId, ts.ObservationTime, ROW_NUMBER()
    OVER (PARTITION BY ts.MeteringPointId, ts.ObservationTime ORDER BY ts.MeteringPointId DESC, ts.ObservationTime DESC) as rownumber from gold.time_series_points as ts) as withRownumber
    where withRownumber.rownumber > 1
  EOT
}

# alert for duplicates in gold.time_series_points
resource "databricks_sql_alert" "duplicates_time_series_gold" {
  name = "Duplicates found in time series gold"
  query_id = databricks_sql_query.duplicates_time_series_gold.id
  rearm = 1
  options {
    column = "rownumber"
    op = "!="
    value = "0"
    muted = false
  }
}

# job for duplicates in wholesale.time_series_points
resource "databricks_job" "duplicates_time_series_gold" {
  name = "Duplicates_time_series_gold"

  schedule {
    quartz_cron_expression = local.alert_trigger_cron
    timezone_id = "UTC"
  }

  email_notifications {
    on_failure = [local.alert_job_email_notification]
  }

  task {
    task_key = "check"

    sql_task {
      warehouse_id = databricks_sql_endpoint.this.id
      alert {
        alert_id = databricks_sql_alert.duplicates_time_series_gold.id
      }
    }
  }
}

# query for duplicates in wholesale.time_series_points
resource "databricks_sql_query" "duplicates_time_series_wholesale" {
  data_source_id = databricks_sql_endpoint.this.data_source_id
  name = "QCTS30-01_duplicates_in_time_series_wholesale"
  query =  <<EOT
    select *
    from (select ts.MeteringPointId, ts.ObservationTime, ROW_NUMBER()
    OVER (PARTITION BY ts.MeteringPointId, ts.ObservationTime ORDER BY ts.MeteringPointId DESC, ts.ObservationTime DESC) as rownumber from wholesale.time_series_points as ts) as withRownumber
    where withRownumber.rownumber > 1
  EOT
}

# alert for duplicates in wholesale.time_series_points
resource "databricks_sql_alert" "duplicates_time_series_wholesale" {
  name = "Duplicates found in time series wholesale"
  query_id = databricks_sql_query.duplicates_time_series_wholesale.id
  rearm = 1
  options {
    column = "rownumber"
    op = "!="
    value = "0"
    muted = false
  }
}

# job for duplicates in wholesale.time_series_points
resource "databricks_job" "duplicates_time_series_wholesale" {
  name = "Duplicates_time_series_wholesale"

  schedule {
    quartz_cron_expression = local.alert_trigger_cron
    timezone_id = "UTC"
  }

  email_notifications {
    on_failure = [local.alert_job_email_notification]
  }

  task {
    task_key = "check"

    sql_task {
      warehouse_id = databricks_sql_endpoint.this.id
      alert {
        alert_id = databricks_sql_alert.duplicates_time_series_wholesale.id
      }
    }
  }
}

# query for duplicates in eloverblik.time_series_points
resource "databricks_sql_query" "duplicates_time_series_eloverblik" {
  data_source_id = databricks_sql_endpoint.this.data_source_id
  name = "QCTS30-01_duplicates_in_time_series_eloverblik"
  query =  <<EOT
  select * from (select ets.metering_point_id, ets.observation_time, ROW_NUMBER()
  OVER (PARTITION BY ets.metering_point_id, ets.observation_time ORDER BY ets.metering_point_id DESC, ets.observation_time DESC) as rownumber
  from eloverblik.eloverblik_time_series_points as ets) as withRownumber where withRownumber.rownumber > 1
  EOT
}

# alert for duplicates in eloverblik.time_series_points
resource "databricks_sql_alert" "duplicates_time_series_eloverblik" {
  name = "Duplicates found in time series eloverblik"
  query_id = databricks_sql_query.duplicates_time_series_eloverblik.id
  rearm = 1
  options {
    column = "rownumber"
    op = "!="
    value = "0"
    muted = false
  }
}

# job for duplicates in eloverblik.time_series_points
resource "databricks_job" "duplicates_time_series_eloverblik" {
  name = "Duplicates_time_series_eloverblik"

  schedule {
    quartz_cron_expression = local.alert_trigger_cron
    timezone_id = "UTC"
  }

  email_notifications {
    on_failure = [local.alert_job_email_notification]
  }

  task {
    task_key = "check"

    sql_task {
      warehouse_id = databricks_sql_endpoint.this.id
      alert {
        alert_id = databricks_sql_alert.duplicates_time_series_eloverblik.id
      }
    }
  }
}
