module "func_receiver" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v10"

  name                                      = "api"
  app_service_plan_id                       = azurerm_service_plan.plan_performance_test.id
  vnet_integration_subnet_id                = data.azurerm_key_vault_secret.snet_vnet_integrations_id_performance_test.value
}