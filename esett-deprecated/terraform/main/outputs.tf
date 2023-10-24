output "ms_esett_deprecated_connection_string" {
  description = "Connection string for executing database migrations on the eSett deprecated database"
  value       = local.connection_string_database_db_migrations
  sensitive   = true
}

output "ms_esett_deprecated_database_name" {
  description = "Database name in the SQL server"
  value       = module.mssqldb_esett.name
  sensitive   = true
}

output "ms_esett_deprecated_database_server" {
  description = "Database server instance hosting the Esett database"
  value       = data.azurerm_key_vault_secret.mssql_data_url.value
  sensitive   = true
}
