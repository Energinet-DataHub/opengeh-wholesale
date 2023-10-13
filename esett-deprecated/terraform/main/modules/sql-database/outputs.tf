﻿output "id" {
  value       = azurerm_mssql_database.main.id
  description = "The SQL Database ID."
}

output "name" {
  value       = azurerm_mssql_database.main.name
  description = "The SQL Database name."
}

output "dependent_on" {
  value       = null_resource.dependency_setter.id
  description = "The SQL Database dependencies."
}
