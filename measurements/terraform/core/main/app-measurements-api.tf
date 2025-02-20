module "app_measurements_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=app-service_8.2.0"

  name                                   = "measurementsapi"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.kvs_snet_vnetintegrations_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.kvs_snet_privateendpoints_id.value
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  app_service_plan_id                    = module.webapp_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  dotnet_framework_version               = "v8.0"
  health_check_path                      = "/monitor/ready"
  app_settings                           = local.app_measurements_api.app_settings
  monitor_action_group = length(module.monitor_action_group_mmcore) != 1 ? null : {
    id                  = module.monitor_action_group_mmcore[0].id
    resource_group_name = azurerm_resource_group.this.name
  }
  role_assignments = [
    {
      resource_id          = module.kv_internal.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    }
  ]
}
