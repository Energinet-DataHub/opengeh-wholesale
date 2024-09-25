resource "databricks_grant" "wholesale_migrations_wholesale_copy" {
  provider          = databricks.dbw
  external_location = databricks_external_location.wholesale_migrations_wholesale.id
  principal         = var.databricks_contributor_dataplane_group.name
  privileges        = ["READ_FILES", "WRITE_FILES"]

  depends_on = [module.dbw]
}

resource "databricks_grant" "wholesale_internal_copy" {
  provider          = databricks.dbw
  external_location = databricks_external_location.internal.id
  principal         = var.databricks_contributor_dataplane_group.name
  privileges        = ["READ_FILES", "WRITE_FILES"]

  depends_on = [module.dbw]
}
