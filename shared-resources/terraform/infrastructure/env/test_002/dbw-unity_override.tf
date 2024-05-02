# TOOD: delete this file when we have the new omada group
resource "databricks_grant" "developers_access_catalog" {
  privileges = ["USE_CATALOG", "SELECT", "READ_VOLUME", "USE_SCHEMA", "CREATE_TABLE", "MODIFY"]
}
