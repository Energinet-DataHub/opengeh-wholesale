output "id" {
  value = azurerm_app_service.main.id
}

output "name" {
  value = azurerm_app_service.main.name
}

output "default_site_hostname" {
  value = azurerm_app_service.main.default_site_hostname
}

output "outbound_ip_addresses" {
  value = azurerm_app_service.main.outbound_ip_addresses
}

output "possible_outbound_ip_addresses" {
  value = azurerm_app_service.main.possible_outbound_ip_addresses
}

output "identity" {
  value = azurerm_app_service.main.identity
}

output "site_credential" {
  value = azurerm_app_service.main.site_credential
}

output "depended_on" {
  value = null_resource.dependency_setter.id
}
