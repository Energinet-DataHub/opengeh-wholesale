module "plan_services" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service-plan?ref=v12"

  name                           = "services"
  project_name                   = var.domain_name_short
  environment_short              = var.environment_short
  environment_instance           = var.environment_instance
  resource_group_name            = azurerm_resource_group.this.name
  location                       = azurerm_resource_group.this.location
  os_type                        = "Windows"
  monitor_alerts_action_group_id = data.azurerm_key_vault_secret.ag_primary_id.value
  sku_name                       = "P1v3"
}
