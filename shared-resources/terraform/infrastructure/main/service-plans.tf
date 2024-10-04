module "webapp_service_plan" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-plan?ref=service-plan_5.0.0"

  type                 = "webapp"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  sku_name             = "P0v3"

  monitor_alerts_action_group_id = length(module.monitor_action_group_shres) != 1 ? null : module.monitor_action_group_shres[0].id

  cpu_alert_information = {
    alerts_enabled = length(module.monitor_action_group_shres) != 1 ? false : true
    frequency      = "PT1M"
    window_size    = "PT5M"
    threshold      = 80
    severity       = 2
  }
  memory_alert_information = {
    alerts_enabled = length(module.monitor_action_group_shres) != 1 ? false : true
    frequency      = "PT1M"
    window_size    = "PT5M"
    threshold      = 80
    severity       = 2
  }
}
