output ms_market_participant_connection_string {
  description = "Connection string of the market participant database created in the shared server"
  value       = local.MS_MARKET_PARTICIPANT_CONNECTION_STRING_SQL_AUTH
  sensitive   = true
}

output ms_market_participant_database_name {
  description = "Database name in the shared sql server"
  value = module.mssqldb_market_participant.name
  sensitive = true  
}

output ms_market_participant_database_server {
  description = "Database server instance hosting the Market Participant database"
  value = data.azurerm_key_vault_secret.mssql_data_url.value
  sensitive = true  
}