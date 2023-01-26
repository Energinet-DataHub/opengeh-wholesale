output ms_marketroles_connection_string {
  description = "Connectionstring to the database in the shared sql server"
  value = local.MS_MARKETROLES_CONNECTION_STRING_SQL_AUTH 
  sensitive = true
}

output ms_marketroles_database_name {
  description = "Database name in the shared sql server"
  value = module.mssqldb_marketroles.name
  sensitive = true  
}

output ms_marketroles_database_server {
  description = "Database server instance hosting the Marketroles database"
  value = data.azurerm_key_vault_secret.mssql_data_url.value
  sensitive = true  
}