resource "azurerm_portal_dashboard" "timeseriesapi" {
  name                = "apd-timeseriesapi-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  dashboard_properties = templatefile("dashboard-templates/timeseriesapi_dashboard.tpl",
    {
      timeseriesapi_subscription_id     = data.azurerm_subscription.this.subscription_id,
      timeseriesapi_resource_group_name = azurerm_resource_group.this.name,
      timeseriesapi_name                = module.app_time_series_api.name,
  })
}
