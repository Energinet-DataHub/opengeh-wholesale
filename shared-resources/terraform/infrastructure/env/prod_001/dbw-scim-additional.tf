# 1109792873721454 is SEC-G-DataHub-BusinessDevelopers
resource "databricks_permission_assignment" "scim_business_developers" {
  provider     = databricks.dbw
  principal_id = "1109792873721454"
  permissions  = ["USER"]

  depends_on = [azurerm_databricks_workspace.this]
}

# Grant business developers access to the catalog
resource "databricks_grant" "developer_access_catalog" {
  provider   = databricks.dbw
  catalog    = databricks_catalog.shared.id
  principal  = "SEC-G-DataHub-BusinessDevelopers"
  privileges = ["USE_CATALOG", "SELECT", "READ_VOLUME", "USE_SCHEMA"]

  depends_on = [azurerm_databricks_workspace.this, databricks_permission_assignment.scim_business_developers]
}
