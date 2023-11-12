output "stapp_platformui_web_app_api_key" {
  description = "Connection string for executing database migrations on the market participant database"
  value       = azurerm_static_site.this.api_key
  sensitive   = true
}

output "ms_platformui_connection_string" {
  description = "Connection string for executing database migrations on the platform ui database"
  value       = local.connection_string_database_db
  sensitive   = true
}
