resource "databricks_permissions" "calculator_job" {
  provider = databricks.dbw
  job_id   = databricks_job.calculator_job.id

  access_control {
    group_name       = "SEC-A-GreenForce-DevelopmentTeamAzure"
    permission_level = "CAN_MANAGE"
  }
}

resource "databricks_permissions" "migrations_job" {
  provider = databricks.dbw
  job_id   = databricks_job.migrations_job.id

  access_control {
    group_name       = "SEC-A-GreenForce-DevelopmentTeamAzure"
    permission_level = "CAN_MANAGE"
  }
}
