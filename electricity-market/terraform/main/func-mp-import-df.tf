module "func_mp_import_df" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app-elastic-durable?ref=function-app-elastic-durable_7.0.0"

  name                                   = "mp-import-df"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  app_service_plan_id                    = module.func_mp_import_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnetintegrations_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  allowed_monitor_reader_entra_groups    = compact([var.developer_security_group_name, var.pim_reader_group_name])
  dotnet_framework_version               = "v8.0"
  use_dotnet_isolated_runtime            = true
  health_check_path                      = "/api/monitor/ready"
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  app_settings                           = local.func_mp_import_df.app_settings

  orchestrations_storage = {
    storage_connection_string = module.taskhub_storage_account.primary_connection_string
    appsettings_name          = "DURABLETASK_STORAGE_CONNECTION_STRING"
  }

  health_check_alert = length(module.monitor_action_group_elmk) != 1 ? null : {
    enabled         = true
    action_group_id = module.monitor_action_group_elmk[0].id
  }

  role_assignments = [
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    }
  ]

  depends_on = [module.taskhub_storage_account]
}
