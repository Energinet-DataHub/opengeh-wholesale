output ms_edi_connection_string {
  description = "Connectionstring to the database in the shared sql server"
  value = local.CONNECTION_STRING_SQL_AUTH 
  sensitive = true
}

output ms_edi_database_name {
  description = "Database name in the shared sql server"
  value = module.mssqldb_edi.name
  sensitive = true  
}

output ms_edi_database_server {
  description = "Database server instance hosting the EDI database"
  value = data.azurerm_key_vault_secret.mssql_data_url.value
  sensitive = true  
}