module "app_dh2_placeholder" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v14"

  name                                   = "dh2-placeholder"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                    = module.webapp_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  health_check_path                      = "/monitor/ready"

  monitor_action_group = length(module.monitor_action_group_dh2bridge) != 1 ? null : {
    id                  = module.monitor_action_group_dh2bridge[0].id
    resource_group_name = azurerm_resource_group.this.name
  }

  dotnet_framework_version = "v8.0"
  ip_restrictions          = var.ip_restrictions
  scm_ip_restrictions      = var.ip_restrictions
}
