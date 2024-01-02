resource "azurerm_portal_dashboard" "timeseriesapi" {
  name                = "apd-timeseriesapi-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  dashboard_properties = templatefile("dashboard-templates/timeseriesapi_dashboard.tpl",
    {
      timeseriesapi_id    = module.app_time_series_api.id,
      timeseriesapi_name  = module.app_time_series_api.name,
      appi_sharedres_id   = data.azurerm_key_vault_secret.appi_id.value,
      appi_sharedres_name = data.azurerm_key_vault_secret.appi_name.value,
      plan_services_id    = data.azurerm_key_vault_secret.plan_shared_id.value,
      plan_services_name  = data.azurerm_key_vault_secret.plan_shared_name.value,
  })
}
