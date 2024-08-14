resource "databricks_sql_global_config" "this" {
  provider        = databricks.dbw
  security_policy = "DATA_ACCESS_CONTROL"
  data_access_config = {
    "spark.hadoop.fs.azure.account.auth.type.${module.st_migrations.name}.dfs.core.windows.net" : "OAuth",
    "spark.hadoop.fs.azure.account.oauth.provider.type.${module.st_migrations.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
    "spark.hadoop.fs.azure.account.oauth2.client.id.${module.st_migrations.name}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference,
    "spark.hadoop.fs.azure.account.oauth2.client.secret.${module.st_migrations.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference,
    "spark.hadoop.fs.azure.account.oauth2.client.endpoint.${module.st_migrations.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${data.azurerm_client_config.this.tenant_id}/oauth2/token",

    "spark.hadoop.fs.azure.account.auth.type.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "OAuth",
    "spark.hadoop.fs.azure.account.oauth.provider.type.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider",
    "spark.hadoop.fs.azure.account.oauth2.client.id.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference,
    "spark.hadoop.fs.azure.account.oauth2.client.secret.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference,
    "spark.hadoop.fs.azure.account.oauth2.client.endpoint.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "https://login.microsoftonline.com/${data.azurerm_client_config.this.tenant_id}/oauth2/token"
  }
}
