resource "databricks_grant" "dev_access_catalog" {
  privileges = ["USE_CATALOG", "SELECT", "READ_VOLUME", "USE_SCHEMA", "CREATE_TABLE", "MODIFY"]
}
