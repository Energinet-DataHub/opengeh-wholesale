module "azapp_biztalkshipper" {
  source                                   = "./modules/azure-windows-web-app-module"
  name                                     = "app-biztalkshipper-${local.name_suffix}"
  resource_group_name                      = azurerm_resource_group.this.name
  location                                 = azurerm_resource_group.this.location
  app_service_plan_id                      = module.plan_services.id
  application_insights_instrumentation_key = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value

  connection_strings = {
    CONNECTION_STRING_SHARED_BLOB = module.stor_esett.primary_connection_string,
  }
}
