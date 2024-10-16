resource "databricks_permissions" "calculator_job" {
  provider = databricks.dbw
  job_id   = databricks_job.calculator_job.id

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

resource "databricks_permissions" "migrations_job" {
  provider = databricks.dbw
  job_id   = databricks_job.migrations_job.id

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

resource "databricks_permissions" "optimise_tables_job" {
  provider = databricks.dbw
  job_id   = databricks_job.optimise_tables_job.id

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

resource "databricks_permissions" "settlement_report_job_balancing" {
  provider = databricks.dbw
  job_id   = databricks_job.settlement_report_job_balancing.id

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

resource "databricks_permissions" "settlement_report_job_balance_fixing" {
  provider = databricks.dbw
  job_id   = databricks_job.settlement_report_job_balance_fixing.id

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

resource "databricks_permissions" "settlement_report_job_wholesale" {
  provider = databricks.dbw
  job_id   = databricks_job.settlement_report_job_wholesale.id

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
