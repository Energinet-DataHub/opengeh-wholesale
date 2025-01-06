module "func_orchestrations" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app-elastic-durable?ref=function-app-elastic-durable_5.0.0"

  name                                   = "orchestrations"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  app_service_plan_id                    = data.azurerm_key_vault_secret.func_service_plan_id.value
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.kvs_snet_vnetintegrations_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.kvs_snet_privateendpoints_id.value
  dotnet_framework_version               = "v8.0"
  use_dotnet_isolated_runtime            = true
  health_check_path                      = "/api/monitor/ready"
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  app_settings                           = local.func_orchestrations.app_settings
  allowed_monitor_reader_entra_groups    = compact([var.developer_security_group_name, var.pim_reader_group_name])

  health_check_alert = var.monitor_action_group_exists == false ? null : {
    action_group_id = data.azurerm_key_vault_secret.monitor_action_group_id[0].value
    enabled         = true
  }
  durabletask_storage_connection_string = "See app setting 'ProcessManagerStorageConnectionString'"

  role_assignments = [
    {
      // Function state storage
      resource_id          = data.azurerm_key_vault_secret.st_taskhub_id.value
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      // Key Vault
      resource_id          = data.azurerm_key_vault.kv_pmcore.id
      role_definition_name = "Key Vault Secrets User"
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
    {
      // ServiceBus EDI Topic
      resource_id          = data.azurerm_key_vault_secret.sbt_edi_id.value
      role_definition_name = "Azure Service Bus Data Owner"
    },
  ]
}

module "kvs_func_orchestrations_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "func-orchestrations-pmorch-base-url"
  value        = "https://${module.func_orchestrations.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
