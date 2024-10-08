module "func_entrypoint_application_workers" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app-elastic?ref=function-app-elastic_7.1.0"

  name                                   = "application-workers"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                    = module.func_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  health_check_path                      = "/api/monitor/ready"

  health_check_alert = length(module.monitor_action_group_esett) != 1 ? null : {
    action_group_id = module.monitor_action_group_esett[0].id
    enabled         = true
  }

  dotnet_framework_version    = "v8.0"
  use_dotnet_isolated_runtime = true
  role_assignments = [
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    },
  ]
  app_settings = local.func_entrypoint_application_workers.app_settings
}
