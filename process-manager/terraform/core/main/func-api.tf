module "func_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app-elastic?ref=function-app-elastic_9.1.1"

  name                                   = "api"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  app_service_plan_id                    = module.func_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.kvs_snet_vnetintegrations_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.kvs_snet_privateendpoints_id.value
  dotnet_framework_version               = "v8.0"
  use_dotnet_isolated_runtime            = true
  health_check_path                      = "/api/monitor/ready"
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  client_certificate_mode                = "Optional"
  app_settings                           = local.func_api.app_settings

  health_check_alert = length(module.monitor_action_group_process_manager) != 1 ? null : {
    action_group_id = module.monitor_action_group_process_manager[0].id
    enabled         = true
  }

  role_assignments = [
    {
      resource_id          = module.st_taskhub.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      // Shared Key Vault
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      // ServiceBus Process Manager Topic
      resource_id          = data.azurerm_key_vault_secret.sbt_processmanager_id.value
      role_definition_name = "Azure Service Bus Data Owner"
    },
  ]
}

module "kvs_func_api_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "func-api-pmcore-base-url"
  value        = "https://${module.func_api.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
