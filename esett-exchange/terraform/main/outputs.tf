output "ms_esett_exchange_connection_string" {
  description = "Connection string to the eSett exchange database created in the shared server."
  value       = local.MS_ESETT_EXCHANGE_CONNECTION_STRING_SQL_AUTH
  sensitive   = true
}

output "ms_esett_exchange_database_name" {
  description = "Database name in the shared server."
  value       = module.mssqldb_esett_exchange.name
  sensitive   = true
}

output "ms_esett_exchange_database_server" {
  description = "Database server instance hosting the eSett exchange database."
  value       = data.azurerm_key_vault_secret.mssql_data_url.value
  sensitive   = true
}
