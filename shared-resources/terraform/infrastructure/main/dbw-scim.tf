locals {
  # Locals determine which security groups that must be assigned grants on the shared Unity catalog.
  # This is necessary as reader and contributor groups may be the same on the development and test environments. In Databricks, the grants of a security group can't be be managed by multiple resources.
  readers = var.databricks_readers_group.name == var.databricks_contributor_dataplane_group.name ? {} : { "${var.databricks_readers_group.name}" = "${var.databricks_readers_group.id}" }
}

# Configure SCIM synchronization or Databricks groups with reader and contributor permissions
resource "databricks_permission_assignment" "scim_reader" {
  for_each     = local.readers
  provider     = databricks.dbw
  principal_id = each.value
  permissions  = ["USER"]

  depends_on = [azurerm_databricks_workspace.this]
}

resource "databricks_permission_assignment" "scim_contributor_dataplane" {
  provider     = databricks.dbw
  principal_id = var.databricks_contributor_dataplane_group.id
  permissions  = ["USER"]

  depends_on = [azurerm_databricks_workspace.this]
}

# Grant access to the reader and contributor groups on the shared catalog
resource "databricks_grant" "reader_access_catalog" {
  for_each   = local.readers
  provider   = databricks.dbw
  catalog    = databricks_catalog.shared.id
  principal  = each.key
  privileges = ["USE_CATALOG", "SELECT", "READ_VOLUME", "USE_SCHEMA"]

  depends_on = [azurerm_databricks_workspace.this, databricks_permission_assignment.scim_reader]
}

resource "databricks_grant" "contributor_dataplane_access_catalog" {
  provider   = databricks.dbw
  catalog    = databricks_catalog.shared.id
  principal  = var.databricks_contributor_dataplane_group.name
  privileges = ["USE_CATALOG", "SELECT", "READ_VOLUME", "USE_SCHEMA", "CREATE_TABLE", "MODIFY", "WRITE_VOLUME"]

  depends_on = [azurerm_databricks_workspace.this, databricks_permission_assignment.scim_contributor_dataplane]
}
