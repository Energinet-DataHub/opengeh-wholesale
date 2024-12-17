module "func_settlement_reports" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=function-app_8.3.0"

  name                                   = "settlement-reports"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  app_service_plan_id                    = module.webapp_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnetintegrations_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  dotnet_framework_version               = "v8.0"
  use_dotnet_isolated_runtime            = true
  health_check_path                      = "/api/monitor/ready"
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  app_settings                           = local.func_settlement_reports.app_settings

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
    },
    {
      // ServiceBus Integration Events Topic
      resource_id          = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_id.value
      role_definition_name = "Azure Service Bus Data Owner"
    }
  ]
}
