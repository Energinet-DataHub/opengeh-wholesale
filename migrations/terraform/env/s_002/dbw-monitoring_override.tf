/*
setting up Databricks jobs for monitoring

adding a sql warehouse
adding a query using the sql warehouse
adding a job to schedule the query
*/

# query for monitor_wholesale_time_series
resource "databricks_sql_query" "monitor_wholesale_time_series" {
  provider = databricks.dbw
}

# query for monitor_wholesale_metering_points
resource "databricks_sql_query" "monitor_wholesale_metering_points" {
  provider = databricks.dbw
}

# query for monitor_eloverblik_time_series
resource "databricks_sql_query" "monitor_eloverblik_time_series" {
  provider = databricks.dbw
}

# job for monitor_wholesale_time_series
resource "databricks_job" "monitor_wholesale_time_series" {
  provider = databricks.dbw
}


# job for monitor_wholesale_metering_points
resource "databricks_job" "monitor_wholesale_metering_points" {
  provider = databricks.dbw
}

# job for monitor_eloverblik_time_series
resource "databricks_job" "monitor_eloverblik_time_series" {
  provider = databricks.dbw
}
