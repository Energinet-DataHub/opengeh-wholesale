module "app_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=app-service_6.1.0"

  name                                   = "api"
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

  monitor_action_group = length(module.monitor_action_group_mkpt) != 1 ? null : {
    id                  = module.monitor_action_group_mkpt[0].id
    resource_group_name = azurerm_resource_group.this.name
  }

  dotnet_framework_version = "v8.0"
  ip_restrictions          = var.ip_restrictions
  scm_ip_restrictions      = var.ip_restrictions
  app_settings             = local.app_api.app_settings

  role_assignments = [
    {
      resource_id          = module.kv_internal.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      resource_id          = module.kv_internal.id
      role_definition_name = "Key Vault Crypto User"
    },
    {
      resource_id          = module.kv_dh2_certificates.id
      role_definition_name = "Key Vault Secrets Officer"
    },
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    }
  ]
}

module "kvs_app_markpart_api_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_5.0.0"

  name         = "app-markpart-api-base-url"
  value        = "https://${module.app_api.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

module "kvs_backend_api_open_id_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_5.0.0"

  name         = "api-backend-open-id-url"
  value        = "https://${module.app_api.default_hostname}/.well-known/openid-configuration"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
