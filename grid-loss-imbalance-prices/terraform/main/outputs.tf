output "ms_grid_loss_imbalance_prices_connection_string" {
  description = "Connection string for executing database migrations on the grid loss imbalance prices database."
  value       = local.CONNECTION_STRING_DB_MIGRATIONS
  sensitive   = true
}

output "ms_grid_loss_imbalance_prices_database_name" {
  description = "Database name in the shared server."
  value       = module.mssqldb_grid_loss_imbalance_prices.name
  sensitive   = true
}

output "ms_grid_loss_imbalance_prices_database_server" {
  description = "Database server instance hosting the grid loss imbalance prices database."
  value       = data.azurerm_key_vault_secret.mssql_data_url.value
  sensitive   = true
}
