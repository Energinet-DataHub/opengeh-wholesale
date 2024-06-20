module "func_service_plan" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-plan?ref=14.22.0"

  type                           = "func"
  project_name                   = var.domain_name_short
  environment_short              = var.environment_short
  environment_instance           = var.environment_instance
  resource_group_name            = azurerm_resource_group.this.name
  location                       = azurerm_resource_group.this.location
  sku_name                       = "EP3"
  maximum_elastic_worker_count   = 4
  monitor_alerts_action_group_id = length(module.monitor_action_group_mig) != 1 ? null : module.monitor_action_group_mig[0].id

  cpu_alert_information = {
    alerts_enabled = length(module.monitor_action_group_mig) != 1 ? false : true
    frequency      = "PT1M"
    window_size    = "PT5M"
    threshold      = 80
    severity       = 2
  }

  memory_alert_information = {
    alerts_enabled = length(module.monitor_action_group_mig) != 1 ? false : true
    frequency      = "PT1M"
    window_size    = "PT5M"
    threshold      = 80
    severity       = 2
  }
}

module "webapp_service_plan" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-plan?ref=14.22.0"

  type                           = "webapp"
  project_name                   = var.domain_name_short
  environment_short              = var.environment_short
  environment_instance           = var.environment_instance
  resource_group_name            = azurerm_resource_group.this.name
  location                       = azurerm_resource_group.this.location
  sku_name                       = "P1v3"
  monitor_alerts_action_group_id = length(module.monitor_action_group_mig) != 1 ? null : module.monitor_action_group_mig[0].id

  cpu_alert_information = {
    alerts_enabled = length(module.monitor_action_group_mig) != 1 ? false : true
    frequency      = "PT1M"
    window_size    = "PT5M"
    threshold      = 80
    severity       = 2
  }

  memory_alert_information = {
    alerts_enabled = length(module.monitor_action_group_mig) != 1 ? false : true
    frequency      = "PT1M"
    window_size    = "PT5M"
    threshold      = 80
    severity       = 2
  }
}
