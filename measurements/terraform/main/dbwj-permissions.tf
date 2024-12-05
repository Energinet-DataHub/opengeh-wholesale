resource "databricks_permissions" "electrical_heating" {
  provider = databricks.dbw
  job_id   = databricks_job.electrical_heating.id

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

resource "databricks_permissions" "effect_payment" {
  provider = databricks.dbw
  job_id   = databricks_job.effect_payment.id

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
