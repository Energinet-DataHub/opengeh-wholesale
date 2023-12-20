/*
setting up Databricks Alerts

adding a query using the Migration endpoint sql warehouse
adding an alert using the query
adding a job to schedule the alert
*/

# query for duplicates in silver.time_series
resource "databricks_sql_query" "duplicates_time_series_silver" {
  provider       = databricks.dbw
  data_source_id = databricks_sql_endpoint.migration_sql_endpoint.data_source_id
  name           = "QCTS07-01_duplicates_in_time_series_silver"
  query          = <<EOT
    select count(*) as duplicates from
    (select ts.metering_point_id, ts.transaction_id, ts.transaction_insert_date, ts.historical_flag, ts.valid_from_date, ts.valid_to_date, ROW_NUMBER()
      OVER (PARTITION BY ts.metering_point_id, ts.transaction_id, ts.transaction_insert_date, ts.historical_flag, ts.valid_from_date, ts.valid_to_date ORDER BY ts.metering_point_id DESC, ts.valid_from_date DESC) as rownumber
      from silver.time_series as ts) as withRownumber
    where withRownumber.rownumber > 1
  EOT
  parent         = "folders/${databricks_directory.shared_queries_dir.object_id}"
}

# alert for duplicates in silver.time_series
resource "databricks_sql_alert" "duplicates_time_series_silver" {
  provider = databricks.dbw
  name     = "Duplicates found in time series silver (QCTS07-01)"
  query_id = databricks_sql_query.duplicates_time_series_silver.id
  rearm    = 1
  options {
    column = "duplicates"
    op     = "!="
    value  = "0"
    muted  = false
  }
  parent = "folders/${databricks_directory.shared_alerts_dir.object_id}"
}

# job for duplicates in silver.time_series
resource "databricks_job" "duplicates_time_series_silver" {
  provider = databricks.dbw
  name     = "Duplicates_time_series_silver"

  schedule {
    quartz_cron_expression = local.alert_trigger_cron
    timezone_id            = "UTC"
  }

  email_notifications {
    on_failure = [var.alert_email_notification]
  }

  task {
    task_key = "check"

    sql_task {
      warehouse_id = databricks_sql_endpoint.migration_sql_endpoint.id
      alert {
        alert_id = databricks_sql_alert.duplicates_time_series_silver.id
      }
    }
  }
}

# query for duplicates in gold.time_series_points
resource "databricks_sql_query" "duplicates_time_series_gold" {
  provider       = databricks.dbw
  data_source_id = databricks_sql_endpoint.migration_sql_endpoint.data_source_id
  name           = "QCTS15-02_duplicates_in_time_series_gold"
  query          = <<EOT
    select count(*) as duplicates
    from (select ts.metering_point_id, ts.observation_time, ROW_NUMBER()
      OVER (PARTITION BY ts.metering_point_id, ts.observation_time ORDER BY ts.metering_point_id DESC, ts.observation_time DESC) as rownumber
      from gold.time_series_points as ts) as withRownumber
    where withRownumber.rownumber > 1
  EOT
  parent         = "folders/${databricks_directory.shared_queries_dir.object_id}"
}

# alert for duplicates in gold.time_series_points
resource "databricks_sql_alert" "duplicates_time_series_gold" {
  provider = databricks.dbw
  name     = "Duplicates found in time series gold (QCTS15-02)"
  query_id = databricks_sql_query.duplicates_time_series_gold.id
  rearm    = 1
  options {
    column = "duplicates"
    op     = "!="
    value  = "0"
    muted  = false
  }
  parent = "folders/${databricks_directory.shared_alerts_dir.object_id}"
}

# job for duplicates in wholesale.time_series_points
resource "databricks_job" "duplicates_time_series_gold" {
  provider = databricks.dbw
  name     = "Duplicates_time_series_gold"

  schedule {
    quartz_cron_expression = local.alert_trigger_cron
    timezone_id            = "UTC"
  }

  email_notifications {
    on_failure = [var.alert_email_notification]
  }

  task {
    task_key = "check"

    sql_task {
      warehouse_id = databricks_sql_endpoint.migration_sql_endpoint.id
      alert {
        alert_id = databricks_sql_alert.duplicates_time_series_gold.id
      }
    }
  }
}

# query for duplicates in wholesale.time_series_points
resource "databricks_sql_query" "duplicates_time_series_wholesale" {
  provider       = databricks.dbw
  data_source_id = databricks_sql_endpoint.migration_sql_endpoint.data_source_id
  name           = "QCTS23-01_duplicates_in_time_series_wholesale"
  query          = <<EOT
    select count(*) as duplicates
    from (select ts.metering_point_id, ts.observation_time, ROW_NUMBER()
      OVER (PARTITION BY ts.metering_point_id, ts.observation_time ORDER BY ts.metering_point_id DESC, ts.observation_time DESC) as rownumber
      from wholesale.time_series_points as ts) as withRownumber
    where withRownumber.rownumber > 1
  EOT
  parent         = "folders/${databricks_directory.shared_queries_dir.object_id}"
}

# alert for duplicates in wholesale.time_series_points
resource "databricks_sql_alert" "duplicates_time_series_wholesale" {
  provider = databricks.dbw
  name     = "Duplicates found in time series wholesale (QCTS23-01)"
  query_id = databricks_sql_query.duplicates_time_series_wholesale.id
  rearm    = 1
  options {
    column = "duplicates"
    op     = "!="
    value  = "0"
    muted  = false
  }
  parent = "folders/${databricks_directory.shared_alerts_dir.object_id}"
}

# job for duplicates in wholesale.time_series_points
resource "databricks_job" "duplicates_time_series_wholesale" {
  provider = databricks.dbw
  name     = "Duplicates_time_series_wholesale"

  schedule {
    quartz_cron_expression = local.alert_trigger_cron
    timezone_id            = "UTC"
  }

  email_notifications {
    on_failure = [var.alert_email_notification]
  }

  task {
    task_key = "check"

    sql_task {
      warehouse_id = databricks_sql_endpoint.migration_sql_endpoint.id
      alert {
        alert_id = databricks_sql_alert.duplicates_time_series_wholesale.id
      }
    }
  }
}

# query for duplicates in eloverblik.time_series_points
resource "databricks_sql_query" "duplicates_time_series_eloverblik" {
  provider       = databricks.dbw
  data_source_id = databricks_sql_endpoint.migration_sql_endpoint.data_source_id
  name           = "QCTS31-01_duplicates_in_time_series_eloverblik"
  query          = <<EOT
  select count(*) as duplicates
  from (select ets.metering_point_id, ets.observation_time, ROW_NUMBER()
    OVER (PARTITION BY ets.metering_point_id, ets.observation_time ORDER BY ets.metering_point_id DESC, ets.observation_time DESC) as rownumber
    from eloverblik.eloverblik_time_series_points as ets) as withRownumber
  where withRownumber.rownumber > 1
  EOT
  parent         = "folders/${databricks_directory.shared_queries_dir.object_id}"
}

# alert for duplicates in eloverblik.time_series_points
resource "databricks_sql_alert" "duplicates_time_series_eloverblik" {
  provider = databricks.dbw
  name     = "Duplicates found in time series eloverblik (QCTS31-01)"
  query_id = databricks_sql_query.duplicates_time_series_eloverblik.id
  rearm    = 1
  options {
    column = "duplicates"
    op     = "!="
    value  = "0"
    muted  = false
  }
  parent = "folders/${databricks_directory.shared_alerts_dir.object_id}"
}

# job for duplicates in eloverblik.time_series_points
resource "databricks_job" "duplicates_time_series_eloverblik" {
  provider = databricks.dbw
  name     = "Duplicates_time_series_eloverblik"

  schedule {
    quartz_cron_expression = local.alert_trigger_cron
    timezone_id            = "UTC"
  }

  email_notifications {
    on_failure = [var.alert_email_notification]
  }

  task {
    task_key = "check"

    sql_task {
      warehouse_id = databricks_sql_endpoint.migration_sql_endpoint.id
      alert {
        alert_id = databricks_sql_alert.duplicates_time_series_eloverblik.id
      }
    }
  }
}

# query for duplicates in gold.metering_points
resource "databricks_sql_query" "duplicates_metering_points_gold" {
  provider       = databricks.dbw
  data_source_id = databricks_sql_endpoint.migration_sql_endpoint.data_source_id
  name           = "QCMP16-01_duplicates_in_metering_points_gold"
  query          = <<EOT
  select count(*) as duplicates
  from (select mp.metering_point_id, mp.valid_from_date, mp.valid_to_date, mp.metering_point_state_id, ROW_NUMBER()
    OVER (PARTITION BY mp.metering_point_id, mp.valid_from_date, mp.valid_to_date, mp.metering_point_state_id ORDER BY
    mp.metering_point_id DESC, mp.valid_from_date DESC, mp.valid_to_date DESC, mp.metering_point_state_id DESC) as rownumber
    from gold.metering_points as mp) as withRownumber
  where withRownumber.rownumber > 1
  EOT
  parent         = "folders/${databricks_directory.shared_queries_dir.object_id}"
}

# alert for duplicates in gold.metering_points
resource "databricks_sql_alert" "duplicates_metering_points_gold" {
  provider = databricks.dbw
  name     = "Duplicates found in metering points gold (QCMP16-01)"
  query_id = databricks_sql_query.duplicates_metering_points_gold.id
  rearm    = 1
  options {
    column = "duplicates"
    op     = "!="
    value  = "0"
    muted  = false
  }
  parent = "folders/${databricks_directory.shared_alerts_dir.object_id}"
}

# job for duplicates in gold.metering_points
resource "databricks_job" "duplicates_metering_points_gold" {
  provider = databricks.dbw
  name     = "Duplicates_metering_points_gold"

  schedule {
    quartz_cron_expression = local.alert_trigger_cron
    timezone_id            = "UTC"
  }

  email_notifications {
    on_failure = [var.alert_email_notification]
  }

  task {
    task_key = "check"

    sql_task {
      warehouse_id = databricks_sql_endpoint.migration_sql_endpoint.id
      alert {
        alert_id = databricks_sql_alert.duplicates_metering_points_gold.id
      }
    }
  }
}

# query for duplicates in wholesale.metering_point_periods
resource "databricks_sql_query" "duplicates_metering_point_periods_wholesale" {
  provider       = databricks.dbw
  data_source_id = databricks_sql_endpoint.migration_sql_endpoint.data_source_id
  name           = "QCMP24-01_duplicates_in_metering_point_periods_wholesale"
  query          = <<EOT
  select count(*) as duplicates
  from (select mp.metering_point_id, mp.from_date, mp.to_date, ROW_NUMBER()
    OVER (PARTITION BY mp.metering_point_id, mp.from_date, mp.to_date ORDER BY mp.metering_point_id DESC, mp.from_date DESC, mp.to_date DESC) as rownumber
    from wholesale.metering_point_periods as mp) as withRownumber
    where withRownumber.rownumber > 1
  EOT
  parent         = "folders/${databricks_directory.shared_queries_dir.object_id}"
}

# alert for duplicates in wholesale.metering_point_periods
resource "databricks_sql_alert" "duplicates_metering_point_periods_wholesale" {
  provider = databricks.dbw
  name     = "Duplicates found in metering point periods wholesale (QCMP24-01)"
  query_id = databricks_sql_query.duplicates_metering_point_periods_wholesale.id
  rearm    = 1
  options {
    column = "duplicates"
    op     = "!="
    value  = "0"
    muted  = false
  }
  parent = "folders/${databricks_directory.shared_alerts_dir.object_id}"
}

# job for duplicates in wholesale.metering_point_periods
resource "databricks_job" "duplicates_metering_point_periods_wholesale" {
  provider = databricks.dbw
  name     = "Duplicates_metering_point_periods_wholesale"

  schedule {
    quartz_cron_expression = local.alert_trigger_cron
    timezone_id            = "UTC"
  }

  email_notifications {
    on_failure = [var.alert_email_notification]
  }

  task {
    task_key = "check"

    sql_task {
      warehouse_id = databricks_sql_endpoint.migration_sql_endpoint.id
      alert {
        alert_id = databricks_sql_alert.duplicates_metering_point_periods_wholesale.id
      }
    }
  }
}
