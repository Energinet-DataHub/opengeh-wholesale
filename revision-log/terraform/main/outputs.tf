output "ms_revision_log_connection_string" {
  description = "Connection string for executing database migrations on the Revision Log database"
  value       = local.CONNECTION_STRING_DB_MIGRATIONS
  sensitive   = true
}

output "ms_revision_log_database_name" {
  description = "Database name in the shared sql server"
  value       = module.mssqldb_revision_log.name
  sensitive   = true
}

output "ms_revision_log_database_server" {
  description = "Database server instance hosting the Revision Log database"
  value       = data.azurerm_key_vault_secret.mssql_data_url.value
  sensitive   = true
}
