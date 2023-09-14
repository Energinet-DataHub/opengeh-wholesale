module "app_health_checks_ui" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v12"

  name                                     = "healthchecksui"
  project_name                             = var.domain_name_short
  environment_short                        = var.environment_short
  environment_instance                     = var.environment_instance
  resource_group_name                      = azurerm_resource_group.this.name
  location                                 = azurerm_resource_group.this.location
  vnet_integration_subnet_id               = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id               = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                      = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  health_check_path                        = "/monitor/ready"
  health_check_alert_action_group_id       = data.azurerm_key_vault_secret.primary_action_group_id.value
  health_check_alert_enabled               = true
  dotnet_framework_version                 = "v7.0"
  ip_restriction_allow_ip_range            = var.hosted_deployagent_public_ip_range

  # Ensure that IHostedServices are not terminated due to unloading of the application in periods with no traffic
  always_on = true

  app_settings = {
    # Health Checks to monitor
    "HealthChecksUI__HealthChecks__0__Name" = "Health Check UI"
    "HealthChecksUI__HealthChecks__0__Uri"  = "/monitor/ready"
    "HealthChecksUI__HealthChecks__1__Name" = "BFF: Web API"
    "HealthChecksUI__HealthChecks__1__Uri"  = "https://app-bff-fe-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__2__Name" = "Market Participant: Web API"
    "HealthChecksUI__HealthChecks__2__Uri"  = "https://app-webapi-markpart-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__3__Name" = "Market Participant: Function"
    "HealthChecksUI__HealthChecks__3__Uri"  = "https://func-organization-markpart-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__4__Name" = "EDI: Function"
    "HealthChecksUI__HealthChecks__4__Uri"  = "https://func-api-edi-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"

    # Polling Interval
    "HealthChecksUI__EvaluationTimeinSeconds" = 60
    # Max. health status history entries returned to UI
    "HealthChecksUI__MaximumExecutionHistoriesPerEndpoint" = 30

    # Database
    # Currently we migrate the database during startup by using SQL Server Authentication.
    # Migration during startup will not work with a managed identity unless we elevate its access (assign it a role that can create/drop tables).
    "HealthChecksUI__DisableMigrations"       = false
    "HealthStatusHistoryDb__ConnectionString" = local.mssql_connection_string

    # Logging
    "Logging__LogLevel__Default"              = "Information"
    "Logging__LogLevel__Microsoft.AspNetCore" = "Warning"
  }
}
