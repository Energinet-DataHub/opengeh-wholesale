module "func_timeseriesprocessor" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app-elastic?ref=function-app-elastic_6.0.0"

  name                                   = "timeseriesprocessor"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  app_service_plan_id                    = module.message_processor_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  dotnet_framework_version               = "v8.0"
  use_dotnet_isolated_runtime            = true
  use_32_bit_worker                      = false
  app_settings                           = local.func_timeseriesprocessor.app_settings
  health_check_path                      = "/api/monitor/ready"
  health_check_alert = length(module.monitor_action_group_mig) != 1 ? null : {
    action_group_id = module.monitor_action_group_mig[0].id
    enabled         = var.enable_health_check_alerts
  }
  role_assignments = [
    {
      resource_id          = module.st_dh2dropzone_archive.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      resource_id          = module.st_dh2data.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      resource_id          = module.st_dh2timeseries_audit.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      resource_id          = module.st_dh2timeseries_intermediary.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      resource_id          = module.kv_internal.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      resource_id          = module.kv_internal.id
      role_definition_name = "Key Vault Crypto User"
    },
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      resource_id          = azurerm_servicebus_topic.time_series_imported_messages_topic.id
      role_definition_name = "Azure Service Bus Data Owner"
    }
  ]
}
