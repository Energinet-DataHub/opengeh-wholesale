output "ms_wholesale_connection_string" {
  description = "Connection string for executing database migrations on the wholesale database"
  value       = local.CONNECTION_STRING_DB_MIGRATIONS
  sensitive   = true
}

output "ms_wholesale_database_name" {
  description = "Database name in the shared sql server"
  value       = module.mssqldb_wholesale.name
  sensitive   = true
}

output "ms_wholesale_database_server" {
  description = "Database server instance hosting the Wholesale database"
  value       = data.azurerm_key_vault_secret.mssql_data_url.value
  sensitive   = true
}

output "dbw_workspace_url" {
  description = "Databricks workspace url"
  value       = module.dbw.workspace_url
  sensitive   = true
}

output "dbw_workspace_token" {
  description = "Databricks workspace token."
  value       = module.dbw.databricks_token
  sensitive   = true
}

output "dbw_databricks_sql_endpoint_id_deployment" {
  description = "Databricks deployment warehouse sql endpoint id."
  value       = resource.databricks_sql_endpoint.deployment_warehouse.id
  sensitive   = true
}

output "st_data_lake_name" {
  description = "Shared storage account data lake name."
  value       = data.azurerm_key_vault_secret.st_data_lake_name.value
  sensitive   = true
}

output "spn_datalake_contributor_app_id" {
  description = "Shared datalake contributor id."
  value       = data.azurerm_key_vault_secret.spn_datalake_contributor_app_id.value
  sensitive   = true
}

output "spn_datalake_contributor_secret" {
  description = "Shared datalake contributor secret."
  value       = data.azurerm_key_vault_secret.spn_datalake_contributor_secret.value
  sensitive   = true
}

output "shared_unity_catalog_name" {
  description = "Databricks shared unity catalog name."
  value       = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  sensitive   = true
}
