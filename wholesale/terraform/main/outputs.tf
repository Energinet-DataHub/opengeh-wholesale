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
