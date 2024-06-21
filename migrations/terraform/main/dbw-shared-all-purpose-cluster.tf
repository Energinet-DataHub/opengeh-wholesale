resource "databricks_cluster" "shared_all_purpose" {
  provider                = databricks.dbw
  cluster_name            = "Shared all-purpose"
  spark_version           = local.databricks_runtime_version
  node_type_id            = "Standard_DS5_v2"
  data_security_mode      = "SINGLE_USER"
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
    "spark.databricks.sql.initial.catalog.name" : data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  }
  spark_env_vars = {
    "APPI_INSTRUMENTATION_KEY"        = databricks_secret.appi_instrumentation_key.config_reference
    "LANDING_STORAGE_ACCOUNT"         = module.st_dh2data.name
    "DATALAKE_STORAGE_ACCOUNT"        = module.st_migrations.name
    "DATALAKE_SHARED_STORAGE_ACCOUNT" = data.azurerm_key_vault_secret.st_data_lake_name.value
    "CATALOG_NAME"                    = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  }
}

resource "databricks_permissions" "cluster_usage" {
  provider   = databricks.dbw
  cluster_id = databricks_cluster.shared_all_purpose.id

  access_control {
    group_name       = "SEC-G-Datahub-DevelopersAzure"
    permission_level = "CAN_MANAGE"
  }
  depends_on = [module.dbw, null_resource.scim_developers]
}
