resource "azurerm_api_management_backend" "edi" {
  name                = "edi"
  resource_group_name = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name = data.azurerm_key_vault_secret.apim_instance_name.value
  protocol            = "http"
  url                 = "https://${module.func_receiver.default_hostname}"
}
