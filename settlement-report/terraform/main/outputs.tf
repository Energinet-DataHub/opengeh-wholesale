output "ms_settlement_report_connection_string" {
  description = "Connection string for executing database migrations on the Settlement Report database"
  value       = local.DB_CONNECTION_STRING
  sensitive   = true
}

output "ms_settlement_report_database_name" {
  description = "Database name in the shared sql server"
  value       = module.mssqldb_settlement_report.name
  sensitive   = true
}

output "ms_settlement_report_database_server" {
  description = "Database server instance hosting the Settlement Report database"
  value       = data.azurerm_key_vault_secret.mssql_data_url.value
  sensitive   = true
}
