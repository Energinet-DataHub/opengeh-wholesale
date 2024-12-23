resource "databricks_secret_scope" "measurements_scope" {
  provider = databricks.dbw
  name     = "measurements-scope"
}

resource "databricks_secret" "spn_app_id" {
  provider     = databricks.dbw
  key          = "spn_app_id"
  string_value = azuread_application.app_databricks.client_id
  scope        = databricks_secret_scope.measurements_scope.id
}

resource "databricks_secret" "spn_app_secret" {
  provider     = databricks.dbw
  key          = "spn_app_secret"
  string_value = module.spn_databricks_rotating_secret.secret
  scope        = databricks_secret_scope.measurements_scope.id
}

resource "databricks_secret" "tenant_id" {
  provider     = databricks.dbw
  key          = "tenant_id"
  string_value = data.azurerm_client_config.this.tenant_id
  scope        = databricks_secret_scope.measurements_scope.id
}
