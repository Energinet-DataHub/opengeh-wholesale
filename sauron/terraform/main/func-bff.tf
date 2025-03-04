module "func_bff_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=function-app_9.0.0"

  name                                   = "bff-api"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnetintegrations_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  app_service_plan_id                    = module.webapp_service_plan.id
  health_check_path                      = "/api/monitor/ready"
  enable_staging_slot                    = true
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  health_check_alert = {
    enabled         = true
    action_group_id = module.monitor_action_group_sauron.id
  }
  ip_restrictions          = var.ip_restrictions
  scm_ip_restrictions      = var.ip_restrictions
  dotnet_framework_version = "v8.0"
  app_settings = {
    CONNECTION_STRING_DATABASE = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
    LOG_ANALYTICS_WORKSPACE_ID = data.azurerm_key_vault_secret.log_analytics_workspace_id.value
  }
}

resource "azurerm_role_assignment" "func_bff_api_log_analytics_reader" {
  principal_id         = module.func_bff_api.identity[0].principal_id
  role_definition_name = "Log Analytics Reader"
  scope                = data.azurerm_key_vault_secret.log_analytics_id.value
}
