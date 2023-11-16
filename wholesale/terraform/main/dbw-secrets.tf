resource "databricks_secret_scope" "wholesale" {
  name = "wholesale-scope"
}

resource "databricks_secret" "spn_app_id" {
  key          = "spn_app_id"
  string_value = azuread_application.app_databricks.application_id
  scope        = databricks_secret_scope.wholesale.id
}

resource "databricks_secret" "spn_app_secret" {
  key          = "spn_app_secret"
  string_value = azuread_application_password.secret.value
  scope        = databricks_secret_scope.wholesale.id
}
