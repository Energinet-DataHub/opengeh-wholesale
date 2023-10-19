output "ms_esett_deprecated_connection_string" {
  description = "Connection string of the Esett database created in the Esett deprecated server"
  value       = local.connection_string_database_sql_auth
  sensitive   = true
}

output "ms_esett_deprecated_database_name" {
  description = "Database name in the Esett deprecated sql server"
  value       = module.mssqldb_esett.name
  sensitive   = true
}

output "ms_esett_deprecated_database_server" {
  description = "Database server instance hosting the Esett database"
  value       = data.azurerm_key_vault_secret.mssql_data_url.value
  sensitive   = true
}
