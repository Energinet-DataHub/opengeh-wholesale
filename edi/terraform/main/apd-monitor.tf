resource "azurerm_portal_dashboard" "monitor" {
  name                = "monitor-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location

  # Temporarily fix: Resource "azurerm_portal_dashboard" stopped working with dashboards based on exported JSON
  # issue: https://github.com/hashicorp/terraform-provider-azurerm/issues/27117
  dashboard_properties = jsonencode(
    merge(
      jsondecode(templatefile("dashboard-templates/edi_monitor.tpl", {
        apim_sharedres_id     = data.azurerm_key_vault_secret.apim_instance_id.value,
        appi_sharedres_id     = data.azurerm_key_vault_secret.appi_shared_id.value,
        apim_b2b_name         = module.apima_b2b.name,
        apim_b2b_ebix_name    = module.apima_b2b_ebix.name,
        sql_db_id             = module.mssqldb_edi.id,
        func_b2b_name         = module.func_receiver.name,
        func_b2b_id           = module.func_receiver.id,
        func_service_plan_id  = module.func_service_plan.id
        app_b2c_id            = module.b2c_web_api.id,
      })),
      {
        "lenses" = {
          for lens_index, lens in jsondecode(templatefile("dashboard-templates/edi_monitor.tpl", {
            apim_sharedres_id     = data.azurerm_key_vault_secret.apim_instance_id.value,
            appi_sharedres_id     = data.azurerm_key_vault_secret.appi_shared_id.value,
            apim_b2b_name         = module.apima_b2b.name,
            apim_b2b_ebix_name    = module.apima_b2b_ebix.name,
            sql_db_id             = module.mssqldb_edi.id,
            func_b2b_name         = module.func_receiver.name,
            func_b2b_id           = module.func_receiver.id,
            func_service_plan_id  = module.func_service_plan.id
            app_b2c_id            = module.b2c_web_api.id,
          })).lenses :
          tostring(lens_index) => merge(lens, {
            "parts" = {
              for part_index, part in lens.parts :
              tostring(part_index) => part
            }
          })
        }
      }
    )
  )
  tags = local.tags
}
