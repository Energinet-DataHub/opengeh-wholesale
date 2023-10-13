output "id" {
  value = azurerm_function_app.main.id
}

output "name" {
  value = azurerm_function_app.main.name
}

output "depended_on" {
  value = null_resource.dependency_setter.id
}

output "default_hostname" {
  value = azurerm_function_app.main.default_hostname
}

output "outbound_ip_addresses" {
  value = azurerm_function_app.main.outbound_ip_addresses
}

output "possible_outbound_ip_addresses" {
  value = azurerm_function_app.main.possible_outbound_ip_addresses
}

output "identity" {
  value = azurerm_function_app.main.identity
}

output "site_credential" {
  value = azurerm_function_app.main.site_credential
}

output "kind" {
  value = azurerm_function_app.main.kind
}
