module "app_ui" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/ui-web-app?ref=ui-web-app_3.0.0"

  name                                   = "ui"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  app_service_plan_id                    = module.webapp_linux_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  app_command_line                       = "node standalone/server.js"
}
