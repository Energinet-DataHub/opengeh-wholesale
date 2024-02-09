resource "azurerm_api_management_backend" "timeseriesapi" {
  name                = "timeseriesapi"
  resource_group_name = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name = data.azurerm_key_vault_secret.apim_instance_name.value
  protocol            = "http"
  url                 = "https://${module.app_time_series_api.default_hostname}"
}
