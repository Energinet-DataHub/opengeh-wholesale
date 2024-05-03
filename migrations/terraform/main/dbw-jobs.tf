locals {
  job_names = [
    "Schema_Migration",
    "Bulk_Charge_Links_Landing_to_Gold",
    "Bulk_Charges_Landing_to_Gold",
    "duplicates_in_time_series_eloverblik",
    "monitor_eloverblik_time_series",
    "duplicates_in_metering_points_gold",
    "Gold_To_Eloverblik",
    "Gold_To_Wholesale",
    "Landing_To_Gold",
    "optimize_bronze_time_series",
    "Reprocess_Metering_Points_Silver_Quarantine",
    "Reprocess_Time_Series_Silver_Quarantine",
    "duplicates_in_time_series_silver",
    "Syncronize_To_Bronze_Time_Series",
    "Stream_Time_Series_Bronze_To_Silver",
    "Stream_Time_Series_Landing_To_Bronze",
    "Stream_Time_Series_Silver_To_Gold",
    "duplicates_in_metering_points_wholesale",
    "monitor_wholesale_metering_points",
    "monitor_wholesale_time_series",
    "Reprocess_Metering_Points_Bronze_Quarantine",
  ]
}

resource "databricks_job" "this" {
  for_each = toset(local.job_names)

  provider = databricks.dbw
  name     = each.value

  lifecycle {
    ignore_changes = all
  }
}

# TOOD: delete this when we have the new omada group
resource "databricks_permissions" "jobs" {
  for_each = toset(local.job_names)

  provider = databricks.dbw
  job_id   = databricks_job.this[each.value].id

  access_control {
    group_name       = "SEC-A-GreenForce-DevelopmentTeamAzure"
    permission_level = "CAN_MANAGE"
  }

  depends_on = [null_resource.scim]
}

resource "databricks_permissions" "jobs_developers" {
  for_each = toset(local.job_names)

  provider = databricks.dbw
  job_id   = databricks_job.this[each.value].id

  access_control {
    group_name       = "SEC-G-Datahub-DevelopersAzure"
    permission_level = "CAN_MANAGE"
  }

  depends_on = [module.dbw, null_resource.scim_developers]
}
