module "app_health_checks_ui" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v13"

  app_settings = {
    # Health Checks to monitor
    "HealthChecksUI__HealthChecks__0__Name" = "Health Check UI"
    "HealthChecksUI__HealthChecks__0__Uri"  = "https://app-healthchecksui-health-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__1__Name" = "BFF: Web API"
    "HealthChecksUI__HealthChecks__1__Uri"  = "https://app-bff-fe-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__2__Name" = "Market Participant: Web API"
    "HealthChecksUI__HealthChecks__2__Uri"  = "https://app-webapi-markpart-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__3__Name" = "Market Participant: Function"
    "HealthChecksUI__HealthChecks__3__Uri"  = "https://func-organization-markpart-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__4__Name" = "EDI: Function"
    "HealthChecksUI__HealthChecks__4__Uri"  = "https://func-api-edi-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"

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
