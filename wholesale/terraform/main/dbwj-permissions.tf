resource "databricks_permissions" "calculator_job" {
  provider = databricks.dbw
  job_id   = databricks_job.calculator_job.id

  access_control {
    group_name       = "SEC-G-Datahub-DevelopersAzure"
    permission_level = "CAN_MANAGE"
  }
  depends_on = [module.dbw, null_resource.scim_developers]
}

resource "databricks_permissions" "migrations_job" {
  provider = databricks.dbw
  job_id   = databricks_job.migrations_job.id

  access_control {
    group_name       = "SEC-G-Datahub-DevelopersAzure"
    permission_level = "CAN_MANAGE"
  }
  depends_on = [module.dbw, null_resource.scim_developers]
}
