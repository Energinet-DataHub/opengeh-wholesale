module "func_settlement_reports_light_df" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app-elastic?ref=function-app-elastic_4.0.1"

  name                                   = "light-df"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  app_service_plan_id                    = module.func_settlement_report_light_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_private_endpoints_002_id.value
  dotnet_framework_version               = "v8.0"
  use_dotnet_isolated_runtime            = true
  is_durable_function                    = true
  health_check_path                      = "/api/monitor/ready"
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  app_settings                           = local.func_settlement_reports_light_df.app_settings

  health_check_alert = length(module.monitor_action_group_setr) != 1 ? null : {
    enabled         = true
    action_group_id = module.monitor_action_group_setr[0].id
  }

  role_assignments = [
    {
      // Blob
      resource_id          = module.storage_settlement_reports.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      // Shared Key Vault
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      // Blob
      resource_id          = data.azurerm_key_vault_secret.st_settlement_report_id.value
      role_definition_name = "Storage Blob Data Reader"
    }
  ]
}

module "kvs_func_settlement_reports_light_df_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_4.0.1"

  name         = "func-settlement-reports-light-df-base-url"
  value        = "https://${module.func_settlement_reports_light_df.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
