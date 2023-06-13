#
# IMPORTANT:
# All 'databricks_' resources in the "Integration Test" environment
# MUST set provider alias; e.g. "provider = databricks.integration_test"
#
provider "databricks" {
  alias  = "integration_test"
  auth_type = "pat"
  host      = "https://${azurerm_databricks_workspace.integration-test-dbw.workspace_url}"
  token     = data.external.databricks_token_integration_test.result.pat_token
}
