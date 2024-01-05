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

output "st_data_lake_name" {
  description = "Shared storage account data lake name."
  value       = data.azurerm_key_vault_secret.st_data_lake_name.value
  sensitive   = true
}

output "st_migrations_data_lake_name" {
  description = "Migrations storage account data lake name."
  value       = module.st_migrations.name
  sensitive   = true
}

output "st_dh2_data_lake_name" {
  description = "Datahub 2 storage account data lake name."
  value       = module.st_dh2data.name
  sensitive   = true
}

output "dbw_workspace_sql_endpoint_id" {
  description = "Databricks workspace sql endpoint id."
  value       = resource.databricks_sql_endpoint.migration_sql_endpoint.id
  sensitive   = true
}
