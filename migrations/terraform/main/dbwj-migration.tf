resource "databricks_secret_scope" "migration_scope" {
  name = "migration-scope"
}

resource "databricks_secret" "spn_app_id" {
  key          = "spn_app_id"
  string_value = azuread_application.app_databricks.application_id
  scope        = databricks_secret_scope.migration_scope.id
}

resource "databricks_secret" "spn_app_secret" {
  key          = "spn_app_secret"
  string_value = azuread_application_password.secret.value
  scope        = databricks_secret_scope.migration_scope.id
}

resource "databricks_secret" "appi_instrumentation_key" {
  key          = "appi_instrumentation_key"
  string_value = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  scope        = databricks_secret_scope.migration_scope.id
}

resource "databricks_secret" "st_dh2data_storage_account" {
  key          = "st_dh2data_storage_account"
  string_value = module.st_dh2data.name
  scope        = databricks_secret_scope.migration_scope.id
}

resource "databricks_secret" "st_shared_datalake_account" {
  key          = "st_shared_datalake_account"
  string_value = data.azurerm_key_vault_secret.st_data_lake_name.value
  scope        = databricks_secret_scope.migration_scope.id
}

resource "databricks_secret" "st_migration_datalake_account" {
  key          = "st_migration_datalake_account"
  string_value = module.st_migrations.name
  scope        = databricks_secret_scope.migration_scope.id
}

resource "databricks_sql_global_config" "this" {
  security_policy = "DATA_ACCESS_CONTROL"
  data_access_config = {
    "spark.hadoop.fs.azure.account.auth.type.${module.st_migrations.name}.dfs.core.windows.net" : "OAuth",
    "spark.hadoop.fs.azure.account.oauth.provider.type.${module.st_migrations.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
    "spark.hadoop.fs.azure.account.oauth2.client.id.${module.st_migrations.name}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference,
    "spark.hadoop.fs.azure.account.oauth2.client.secret.${module.st_migrations.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference,
    "spark.hadoop.fs.azure.account.oauth2.client.endpoint.${module.st_migrations.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${var.tenant_id}/oauth2/token",

    "spark.hadoop.fs.azure.account.auth.type.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "OAuth",
    "spark.hadoop.fs.azure.account.oauth.provider.type.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider",
    "spark.hadoop.fs.azure.account.oauth2.client.id.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference,
    "spark.hadoop.fs.azure.account.oauth2.client.secret.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference,
    "spark.hadoop.fs.azure.account.oauth2.client.endpoint.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "https://login.microsoftonline.com/${var.tenant_id}/oauth2/token"
  }
}
