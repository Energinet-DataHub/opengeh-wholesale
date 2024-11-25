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
    group_name       = var.databricks_contributor_dataplane_group.name
    permission_level = "CAN_MANAGE"
  }
  dynamic "access_control" {
    for_each = local.readers
    content {
      group_name       = access_control.key
      permission_level = "CAN_VIEW"
    }
  }

  depends_on = [module.dbw]
}
