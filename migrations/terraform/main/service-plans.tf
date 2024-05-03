module "func_service_plan" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-plan?ref=v14"

  type                           = "func"
  project_name                   = var.domain_name_short
  environment_short              = var.environment_short
  environment_instance           = var.environment_instance
  resource_group_name            = data.azurerm_resource_group.shared.name
  location                       = data.azurerm_resource_group.shared.location
  sku_name                       = "EP1"
  maximum_elastic_worker_count   = 20
  monitor_alerts_action_group_id = module.monitor_action_group.id
  cpu_alert_information = {
    alerts_enabled = true
    frequency      = "PT1M"
    window_size    = "PT5M"
    threshold      = 80
    severity       = 2
  }
  memory_alert_information = {
    alerts_enabled = true
    frequency      = "PT1M"
    window_size    = "PT5M"
    threshold      = 80
    severity       = 2
  }
}

module "webapp_service_plan" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-plan?ref=v14"

  type                           = "webapp"
  project_name                   = var.domain_name_short
  environment_short              = var.environment_short
  environment_instance           = var.environment_instance
  resource_group_name            = data.azurerm_resource_group.shared.name
  location                       = data.azurerm_resource_group.shared.location
  sku_name                       = "P0v3"
  monitor_alerts_action_group_id = module.monitor_action_group.id
  cpu_alert_information = {
    alerts_enabled = true
    frequency      = "PT1M"
    window_size    = "PT5M"
    threshold      = 80
    severity       = 2
  }
  memory_alert_information = {
    alerts_enabled = true
    frequency      = "PT1M"
    window_size    = "PT5M"
    threshold      = 80
    severity       = 2
  }
}
