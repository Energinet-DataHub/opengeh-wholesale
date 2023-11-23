/*
setting up Databricks Alerts

adding a query using the Migration endpoint sql warehouse
adding an alert using the query
adding a job to schedule the alert
*/

# query for duplicates in silver.time_series
resource "databricks_sql_query" "duplicates_time_series_silver" {
  provider = databricks.dbw
}

# alert for duplicates in silver.time_series
resource "databricks_sql_alert" "duplicates_time_series_silver" {
  provider = databricks.dbw
}

# job for duplicates in silver.time_series
resource "databricks_job" "duplicates_time_series_silver" {
  provider = databricks.dbw
}

# query for duplicates in gold.time_series_points
resource "databricks_sql_query" "duplicates_time_series_gold" {
  provider = databricks.dbw
}

# alert for duplicates in gold.time_series_points
resource "databricks_sql_alert" "duplicates_time_series_gold" {
  provider = databricks.dbw
}

# job for duplicates in wholesale.time_series_points
resource "databricks_job" "duplicates_time_series_gold" {
  provider = databricks.dbw
}

# query for duplicates in wholesale.time_series_points
resource "databricks_sql_query" "duplicates_time_series_wholesale" {
  provider = databricks.dbw
}

# alert for duplicates in wholesale.time_series_points
resource "databricks_sql_alert" "duplicates_time_series_wholesale" {
  provider = databricks.dbw
}

# job for duplicates in wholesale.time_series_points
resource "databricks_job" "duplicates_time_series_wholesale" {
  provider = databricks.dbw
}

# query for duplicates in eloverblik.time_series_points
resource "databricks_sql_query" "duplicates_time_series_eloverblik" {
  provider = databricks.dbw
}

# alert for duplicates in eloverblik.time_series_points
resource "databricks_sql_alert" "duplicates_time_series_eloverblik" {
  provider = databricks.dbw
}

# job for duplicates in eloverblik.time_series_points
resource "databricks_job" "duplicates_time_series_eloverblik" {
  provider = databricks.dbw
}

# query for duplicates in gold.metering_points
resource "databricks_sql_query" "duplicates_metering_points_gold" {
  provider = databricks.dbw
}

# alert for duplicates in gold.metering_points
resource "databricks_sql_alert" "duplicates_metering_points_gold" {
  provider = databricks.dbw
}

# job for duplicates in gold.metering_points
resource "databricks_job" "duplicates_metering_points_gold" {
  provider = databricks.dbw
}

# query for duplicates in wholesale.metering_point_periods
resource "databricks_sql_query" "duplicates_metering_point_periods_wholesale" {
  provider = databricks.dbw
}

# alert for duplicates in wholesale.metering_point_periods
resource "databricks_sql_alert" "duplicates_metering_point_periods_wholesale" {
  provider = databricks.dbw
}

# job for duplicates in wholesale.metering_point_periods
resource "databricks_job" "duplicates_metering_point_periods_wholesale" {
  provider = databricks.dbw
}
