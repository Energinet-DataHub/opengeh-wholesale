module "func_orchestrationsdf" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app-elastic-durable?ref=function-app-elastic-durable_6.0.0"

  name                                   = "orchestrationsdf"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  app_service_plan_id                    = module.func_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnetintegrations_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  dotnet_framework_version               = "v8.0"
  use_dotnet_isolated_runtime            = true
  orchestrations_storage = {
    storage_connection_string = "See_app_setting_OrchestrationsStorageConnectionString"
    appsettings_name          = "DURABLETASK_STORAGE_CONNECTION_STRING"
  }
  health_check_path                   = "/api/monitor/ready"
  ip_restrictions                     = var.ip_restrictions
  scm_ip_restrictions                 = var.ip_restrictions
  app_settings                        = local.func_orchestrationsdf.app_settings
  allowed_monitor_reader_entra_groups = compact([var.developer_security_group_name, var.pim_reader_group_name])

  health_check_alert = length(module.monitor_action_group_wholesale) != 1 ? null : {
    enabled         = true
    action_group_id = module.monitor_action_group_wholesale[0].id
  }

  role_assignments = [
    {
      // DataLake
      resource_id          = data.azurerm_key_vault_secret.st_data_lake_id.value
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
    {
      // ServiceBus Integration Events Topic
      resource_id          = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_id.value
      role_definition_name = "Azure Service Bus Data Owner"
    },
    {
      // ServiceBus Wholesale Inbox Queue
      resource_id          = data.azurerm_key_vault_secret.sbq_wholesale_inbox_id.value
      role_definition_name = "Azure Service Bus Data Owner"
    },
    {
      // ServiceBus EDI Inbox Queue
      resource_id          = data.azurerm_key_vault_secret.sbq_edi_inbox_id.value
      role_definition_name = "Azure Service Bus Data Owner"
    },
    {
      // Dead-letter logs
      resource_id          = data.azurerm_key_vault_secret.st_deadltr_shres_id.value
      role_definition_name = "Storage Blob Data Contributor"
    },
  ]
}

module "kvs_func_orchestrationsdf_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "func-wholesale-orchestrationsdf-base-url"
  value        = "https://${module.func_orchestrationsdf.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

