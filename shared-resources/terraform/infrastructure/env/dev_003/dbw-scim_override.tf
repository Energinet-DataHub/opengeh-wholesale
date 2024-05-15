# TOOD: delete this when we have the new omada group
resource "databricks_grant" "dev_access_catalog" {
  privileges = ["USE_CATALOG", "SELECT", "READ_VOLUME", "USE_SCHEMA", "CREATE_TABLE", "MODIFY"]
}

resource "databricks_grant" "developer_access_catalog" {
  privileges = ["USE_CATALOG", "SELECT", "READ_VOLUME", "USE_SCHEMA", "CREATE_TABLE", "MODIFY"]
}
