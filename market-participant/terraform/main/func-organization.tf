module "func_organization" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=function-app_8.3.0"

  name                                   = "organization"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnetintegrations_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  app_service_plan_id                    = module.webapp_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  health_check_path                      = "/api/monitor/ready"
  always_on                              = true
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions

  health_check_alert = length(module.monitor_action_group_mkpt) != 1 ? null : {
    action_group_id = module.monitor_action_group_mkpt[0].id
    enabled         = true
  }

  dotnet_framework_version    = "v8.0"
  use_dotnet_isolated_runtime = true
  app_settings                = local.func_organization.app_settings

  role_assignments = [
    {
      resource_id          = module.kv_internal.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      resource_id          = module.kv_dh2_certificates.id
      role_definition_name = "Key Vault Secrets Officer"
    },
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      // ServiceBus Integration Events Topic
      resource_id          = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_id.value
      role_definition_name = "Azure Service Bus Data Owner"
    }
  ]
}
