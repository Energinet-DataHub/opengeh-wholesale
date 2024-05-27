resource "databricks_job" "this" {
  for_each = toset(local.job_names)

  provider = databricks.dbw
  name     = each.value

  lifecycle {
    ignore_changes = all
  }
}

resource "databricks_permissions" "jobs" {
  for_each = toset(local.job_names)

  provider = databricks.dbw
  job_id   = databricks_job.this[each.value].id

  access_control { # TOOD: delete this when we have the new omada group
    group_name       = "SEC-A-GreenForce-DevelopmentTeamAzure"
    permission_level = "CAN_MANAGE"
  }

  access_control {
    group_name       = "SEC-G-Datahub-DevelopersAzure"
    permission_level = "CAN_MANAGE"
  }

  depends_on = [module.dbw, null_resource.scim, null_resource.scim_developers]
}
