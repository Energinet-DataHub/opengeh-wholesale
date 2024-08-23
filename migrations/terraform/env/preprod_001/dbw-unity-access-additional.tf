resource "databricks_grant" "shared_wholesale_input" {
  provider = databricks.dbw
  schema   = databricks_schema.shared_wholesale_input.id

  principal  = "SEC-G-Datahub-DevelopersAzure"
  privileges = ["USE_SCHEMA", "MODIFY", "SELECT", "REFRESH", "EXECUTE", "CREATE_TABLE"]

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}
