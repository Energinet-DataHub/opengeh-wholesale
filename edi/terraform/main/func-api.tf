module "func_receiver" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app-elastic?ref=14.22.0"

  name                        = "api"
  project_name                = var.domain_name_short
  environment_short           = var.environment_short
  environment_instance        = var.environment_instance
  resource_group_name         = azurerm_resource_group.this.name
  location                    = azurerm_resource_group.this.location
  app_service_plan_id         = module.func_service_plan.id
  vnet_integration_subnet_id  = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id  = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  dotnet_framework_version    = "v8.0"
  use_dotnet_isolated_runtime = true
  is_durable_function         = true
  health_check_path           = "/api/monitor/ready"

  health_check_alert = length(module.monitor_action_group_edi) != 1 ? null : {
    action_group_id = module.monitor_action_group_edi[0].id
    enabled         = true
  }

  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  client_certificate_mode                = "Optional"
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  app_settings                           = local.func_receiver.app_settings

  # Role assigments is needed to connect to the storage account (st_documents) using URI
  role_assignments = [
    {
      resource_id          = module.st_documents.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    }
  ]
}

module "kvs_edi_api_base_url" {
  source       = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=14.22.0"
  name         = "func-edi-api-base-url"
  value        = "https://${module.func_receiver.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
