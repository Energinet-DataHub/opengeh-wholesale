module "func_process_manager_orchestrations" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app-elastic-durable?ref=function-app-elastic-durable_5.0.0"

  name                                   = "pm-orchestrations"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  app_service_plan_id                    = module.func_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  dotnet_framework_version               = "v8.0"
  use_dotnet_isolated_runtime            = true
  health_check_path                      = "/api/monitor/ready"
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  app_settings                           = local.func_process_manager_orchestrations.app_settings
  allowed_monitor_reader_entra_groups    = [var.developer_security_group_name]

  health_check_alert = length(module.monitor_action_group_edi) != 1 ? null : {
    action_group_id = module.monitor_action_group_edi[0].id
    enabled         = true
  }
  durabletask_storage_connection_string = "See app setting 'ProcessManagerStorageConnectionString'"

  role_assignments = [
    {
      // Function state storage
      resource_id          = module.st_process_manager.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      // Key Vault
      resource_id          = module.kv_internal.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      // Shared Key Vault
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    },
  ]
}

module "kvs_func_process_manager_orchestrations_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "func-pm-orchestrations-edi-base-url"
  value        = "https://${module.func_process_manager_orchestrations.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
