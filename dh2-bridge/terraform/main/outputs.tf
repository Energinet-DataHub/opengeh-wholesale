output "ms_dh2_bridge_connection_string" {
  description = "Connection string for executing database migrations on the dh2 bridge database"
  value       = module.kvs_sql_connection_string_db_migrations.value
  sensitive   = true
}

output "ms_dh2_bridge_database_name" {
  description = "Database name in the shared server."
  value       = module.mssqldb_dh2_bridge.name
  sensitive   = true
}

output "ms_dh2_bridge_database_server" {
  description = "Database server instance hosting the dh2 bridge database."
  value       = data.azurerm_key_vault_secret.mssql_data_url.value
  sensitive   = true
}
