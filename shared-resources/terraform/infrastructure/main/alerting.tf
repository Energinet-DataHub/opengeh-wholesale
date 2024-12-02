module "monitor_action_group_shres" {
  count = var.alert_email_address != null ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=monitor-action-group-email_6.0.1"

  name                 = "alerts"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name             = "sres-alerts"
  email_receiver_name    = "Shared Resources Operations"
  email_receiver_address = var.alert_email_address

  default_query_exceptions_errors = {
    enabled = false
  }
  default_query_request_errors = {
    enabled = false
  }

  application_insights_id = module.appi_shared.id
}

module "monitor_action_group_platform" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=monitor-action-group-email_6.0.1"

  name                 = "platform-alerts"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name             = "plat-alerts"
  email_receiver_name    = "Platform Operations"
  email_receiver_address = "ff263329.energinet.onmicrosoft.com@emea.teams.ms"

  default_query_exceptions_errors = {
    enabled = false
  }
  default_query_request_errors = {
    enabled = false
  }

  application_insights_id = module.appi_shared.id
}
