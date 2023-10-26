output "ms_market_participant_connection_string" {
  description = "Connection string for executing database migrations on the market participant database"
  value       = local.CONNECTION_STRING_DB_MIGRATIONS
  sensitive   = true
}

output "ms_market_participant_database_name" {
  description = "Database name in the shared sql server"
  value       = module.mssqldb_market_participant.name
  sensitive   = true
}

output "ms_market_participant_database_server" {
  description = "Database server instance hosting the Market Participant database"
  value       = data.azurerm_key_vault_secret.mssql_data_url.value
  sensitive   = true
}
