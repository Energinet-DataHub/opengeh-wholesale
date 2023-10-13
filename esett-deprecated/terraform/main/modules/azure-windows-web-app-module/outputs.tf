output "id" {
  value = azurerm_windows_web_app.main.id
}

output "name" {
  value = azurerm_windows_web_app.main.name
}

output "default_hostname" {
  value = azurerm_windows_web_app.main.default_hostname
}

output "outbound_ip_addresses" {
  value = azurerm_windows_web_app.main.outbound_ip_addresses
}

output "possible_outbound_ip_addresses" {
  value = azurerm_windows_web_app.main.possible_outbound_ip_addresses
}

output "identity" {
  value = azurerm_windows_web_app.main.identity
}

output "site_credential" {
  value = azurerm_windows_web_app.main.site_credential
}

output "depended_on" {
  value = null_resource.dependency_setter.id
}
