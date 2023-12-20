resource "databricks_cluster" "shared_all_purpose" {
  provider                = databricks.dbw
  cluster_name            = "Shared all-purpose"
  spark_version           = data.databricks_spark_version.latest_lts.id
  node_type_id            = "Standard_DS5_v2"
  autotermination_minutes = 15
  num_workers             = 1
  spark_conf = {
    "fs.azure.account.oauth2.client.endpoint.${module.st_dh2data.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${var.tenant_id}/oauth2/token"
    "fs.azure.account.oauth2.client.endpoint.${module.st_migrations.name}.dfs.core.windows.net" : "https://login.microsoftonline.com/${var.tenant_id}/oauth2/token"
    "fs.azure.account.oauth2.client.endpoint.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "https://login.microsoftonline.com/${var.tenant_id}/oauth2/token"
    "fs.azure.account.auth.type.${module.st_dh2data.name}.dfs.core.windows.net" : "OAuth"
    "fs.azure.account.auth.type.${module.st_migrations.name}.dfs.core.windows.net" : "OAuth"
    "fs.azure.account.auth.type.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "OAuth"
    "fs.azure.account.oauth.provider.type.${module.st_dh2data.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
    "fs.azure.account.oauth.provider.type.${module.st_migrations.name}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
    "fs.azure.account.oauth.provider.type.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
    "fs.azure.account.oauth2.client.id.${module.st_dh2data.name}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference
    "fs.azure.account.oauth2.client.id.${module.st_migrations.name}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference
    "fs.azure.account.oauth2.client.id.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference
    "fs.azure.account.oauth2.client.secret.${module.st_dh2data.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference
    "fs.azure.account.oauth2.client.secret.${module.st_migrations.name}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference
    "fs.azure.account.oauth2.client.secret.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference
    "spark.databricks.delta.preview.enabled" : true
    "spark.databricks.io.cache.enabled" : true
    "spark.master" : "local[*, 4]"
  }
  spark_env_vars = {
    "APPI_INSTRUMENTATION_KEY"        = databricks_secret.appi_instrumentation_key.config_reference
    "LANDING_STORAGE_ACCOUNT"         = module.st_dh2data.name
    "DATALAKE_STORAGE_ACCOUNT"        = module.st_migrations.name
    "DATALAKE_SHARED_STORAGE_ACCOUNT" = data.azurerm_key_vault_secret.st_data_lake_name.value
  }
}
