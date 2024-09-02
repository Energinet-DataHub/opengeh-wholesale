resource "databricks_permissions" "calculator_job_reader" {
  for_each = local.readers
  provider = databricks.dbw
  job_id   = databricks_job.calculator_job.id

  access_control {
    group_name       = each.key
    permission_level = "CAN_VIEW"
  }
  depends_on = [module.dbw]
}

resource "databricks_permissions" "calculator_job_contributor_dataplane" {
  provider = databricks.dbw
  job_id   = databricks_job.calculator_job.id

  access_control {
    group_name       = var.databricks_contributor_dataplane_group.name
    permission_level = "CAN_MANAGE"
  }
  depends_on = [module.dbw]
}

resource "databricks_permissions" "migrations_job_reader" {
  for_each = local.readers
  provider = databricks.dbw
  job_id   = databricks_job.migrations_job.id

  access_control {
    group_name       = each.key
    permission_level = "CAN_VIEW"
  }
  depends_on = [module.dbw]
}

resource "databricks_permissions" "migrations_job_contributor_dataplane" {
  provider = databricks.dbw
  job_id   = databricks_job.migrations_job.id

  access_control {
    group_name       = var.databricks_contributor_dataplane_group.name
    permission_level = "CAN_MANAGE"
  }
  depends_on = [module.dbw]
}

resource "databricks_permissions" "optimise_tables_job_reader" {
  for_each = local.readers
  provider = databricks.dbw
  job_id   = databricks_job.optimise_tables_job.id

  access_control {
    group_name       = each.key
    permission_level = "CAN_VIEW"
  }
  depends_on = [module.dbw]
}

resource "databricks_permissions" "optimise_tables_job_contributor_dataplane" {
  provider = databricks.dbw
  job_id   = databricks_job.optimise_tables_job.id

  access_control {
    group_name       = var.databricks_contributor_dataplane_group.name
    permission_level = "CAN_MANAGE"
  }
  depends_on = [module.dbw]
}
