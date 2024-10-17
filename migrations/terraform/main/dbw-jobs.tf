resource "databricks_job" "this" {
  for_each = toset(local.job_names)

  provider = databricks.dbw
  name     = each.value

  # Lifecycle is ignored, as migrations update the jobs in the migrations repository using APIs
  lifecycle {
    ignore_changes = all
  }
}

resource "databricks_permissions" "jobs" {
  for_each = toset(local.job_names)

  provider = databricks.dbw
  job_id   = databricks_job.this[each.value].id

  access_control {
    group_name       = "SEC-G-Datahub-DevelopersAzure"
    permission_level = "CAN_MANAGE"
  }

  depends_on = [module.dbw, null_resource.scim_developers]
}
