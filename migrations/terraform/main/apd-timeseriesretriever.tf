resource "azurerm_portal_dashboard" "timeseriesretriever" {
  name                = "apd-timeseriesretriever-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  dashboard_properties = templatefile("dashboard-templates/timeseriessync_dashboard.tpl",
    {
      timeseriessync_id   = module.func_timeseriesretriever_v2.id,
      timeseriessync_name = module.func_timeseriesretriever_v2.name,
      appi_sharedres_id   = data.azurerm_key_vault_secret.appi_id.value,
      appi_sharedres_name = data.azurerm_key_vault_secret.appi_name.value,
      plan_services_id    = module.message_retriever_service_plan.id
      plan_services_name  = module.message_retriever_service_plan.name
      sbns_shared_id      = data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value,
      sbns_shared_name    = data.azurerm_key_vault_secret.sb_domain_relay_namespace_name.value,
  })

  tags = local.tags
}
