# resource "azurerm_portal_dashboard" "timeseriessync" {
#   name                = "apd-timeseriessync-${local.resources_suffix}"
#   resource_group_name = azurerm_resource_group.this.name
#   location            = azurerm_resource_group.this.location
#   dashboard_properties = templatefile("dashboard-templates/timeseriessync_dashboard.tpl",
#     {
#       timeseriessync_id   = module.func_timeseriessynchronization.id,
#       timeseriessync_name = module.func_timeseriessynchronization.name,
#       appi_sharedres_id   = data.azurerm_key_vault_secret.appi_id.value,
#       appi_sharedres_name = data.azurerm_key_vault_secret.appi_name.value,
#       sbns_shared_id      = data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value,
#       sbns_shared_name    = data.azurerm_key_vault_secret.sb_domain_relay_namespace_name.value,
#       plan_services_id    = data.azurerm_key_vault_secret.plan_shared_id.value,
#       plan_services_name  = data.azurerm_key_vault_secret.plan_shared_name.value,
#   })
# }
