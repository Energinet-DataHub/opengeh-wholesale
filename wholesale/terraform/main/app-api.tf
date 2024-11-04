module "app_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=app-service_7.0.1"

  name                                   = "api"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  app_service_plan_id                    = module.webapp_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  dotnet_framework_version               = "v8.0"
  health_check_path                      = "/monitor/ready"
  app_settings                           = local.app_api.app_settings
  connection_strings                     = local.app_api.connection_strings
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  # Always on would make Azure poll /GET frequently to keep the app warm.
  # But (1) that endpoint doesn't exist and generates 404 responses, and (2) it's not needed as the app is being kept warm by the health checks
  always_on = false

  monitor_action_group = length(module.monitor_action_group_wholesale) == 1 ? {
    id                  = module.monitor_action_group_wholesale[0].id
    resource_group_name = azurerm_resource_group.this.name
  } : null

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
      // ServiceBus Integration Events Subscription
      resource_id          = module.sbtsub_wholesale_integration_event_listener.id
      role_definition_name = "Azure Service Bus Data Receiver"
    }
  ]
}

module "kvs_app_api_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "app-wholesale-webapi-base-url"
  value        = "https://${module.app_api.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
