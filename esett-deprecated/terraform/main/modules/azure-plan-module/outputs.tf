output "id" {
  value       = azurerm_app_service_plan.main.id
  description = "The ID of the Application Service plpan."
}

output "name" {
  value       = azurerm_app_service_plan.main.name
  description = "The name of the Application Service plpan."
}

output "depended_on" {
  value = null_resource.dependency_setter.id
}

output "maximum_number_of_workers" {
  value = azurerm_app_service_plan.main.maximum_number_of_workers
}
