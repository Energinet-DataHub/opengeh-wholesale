module "plan_services" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service-plan?ref=13.32.0"

  name                           = "services"
  project_name                   = var.domain_name_short
  environment_short              = var.environment_short
  environment_instance           = var.environment_instance
  resource_group_name            = azurerm_resource_group.this.name
  location                       = azurerm_resource_group.this.location
  os_type                        = "Windows"
  monitor_alerts_action_group_id = module.ag_primary.id
  sku_name                       = "P1v3"
}

module "kvs_plan_services_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "plan-services-id"
  value        = module.plan_services.id
  key_vault_id = module.kv_shared.id
}

module "kvs_plan_services_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "plan-services-name"
  value        = module.plan_services.name
  key_vault_id = module.kv_shared.id
}
