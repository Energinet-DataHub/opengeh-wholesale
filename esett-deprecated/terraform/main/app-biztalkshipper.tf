module "app_biztalkshipper" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v13-without-vnet"

  name                                      = "biztalkshipper"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  app_service_plan_id                       = module.plan_services.id
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  connection_strings = [
    {
      name = "CONNECTION_STRING_SHARED_BLOB"
      type = "Custom"
      value = module.stor_esett.primary_connection_string
    }
  ]
}
