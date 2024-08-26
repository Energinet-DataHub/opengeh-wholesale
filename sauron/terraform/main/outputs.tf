output "ms_sauron_connection_string" {
  description = "Connection string for executing database migrations on the sauron database"
  value       = local.connection_string_database_db
  sensitive   = true
}
