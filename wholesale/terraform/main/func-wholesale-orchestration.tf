module "func_wholesale_orchestration" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v13"

  name                                     = "orchestration"
  project_name                             = var.domain_name_short
  environment_short                        = var.environment_short
  environment_instance                     = var.environment_instance
  resource_group_name                      = azurerm_resource_group.this.name
  location                                 = azurerm_resource_group.this.location
  app_service_plan_id                      = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_connection_string   = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  vnet_integration_subnet_id               = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id               = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  always_on                                = true
  dotnet_framework_version                 = "v8.0"
  use_dotnet_isolated_runtime              = true
  health_check_path                        = "/api/monitor/ready"
  ip_restrictions                          = var.ip_restrictions
  scm_ip_restrictions                      = var.ip_restrictions

  app_settings = {
    # Logging
    "Logging__ApplicationInsights__LogLevel__Default"                     = local.LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT
    "Logging__ApplicationInsights__LogLevel__Energinet.Datahub.Wholesale" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_WHOLESALE
    "Logging__ApplicationInsights__LogLevel__Energinet.Datahub.Core"      = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE
  }
}
