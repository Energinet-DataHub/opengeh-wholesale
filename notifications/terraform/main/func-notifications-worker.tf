module "func_notifications_worker" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app-elastic?ref=function-app-elastic_7.1.0"

  name                                   = "notifications-worker"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_private_endpoints_002_id.value
  app_service_plan_id                    = module.func_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  health_check_path                      = "/api/monitor/ready"
  dotnet_framework_version               = "v8.0"
  use_dotnet_isolated_runtime            = true
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  app_settings                           = local.func_notifications_worker.app_settings

  health_check_alert = length(module.monitor_action_group) != 1 ? null : {
    action_group_id = module.monitor_action_group[0].id
    enabled         = true
  }

  role_assignments = [
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      // ServiceBus Integration Events Topic
      resource_id          = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_id.value
      role_definition_name = "Azure Service Bus Data Owner"
    }
  ]
}

module "kvs_func_notifications_worker_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_5.0.0"

  name         = "func-notifications-worker-base-url"
  value        = "https://${module.func_notifications_worker.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
