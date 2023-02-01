module "app_performancetest" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v10"

  name                                      = "performancetest"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  vnet_integration_subnet_id                = data.azurerm_key_vault_secret.snet_vnet_integrations_id_performance_test.value
  private_endpoint_subnet_id                = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                       = azurerm_service_plan.plan_performance_test.id
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  dotnet_framework_version                  = "v6.0"

  app_settings                              = {
     MessagingApi__Hostname                 = "${module.func_receiver.name}.azurewebsites.net"
     MessagingApi__Port                     = 443
  }
}
