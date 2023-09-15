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
    "HealthChecksUI__HealthChecks__5__Name" = "Migration: Dropzoneunzipper Function"
    "HealthChecksUI__HealthChecks__5__Uri"  = "https://func-dropzoneunzipper-mig-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__6__Name" = "Migration: Migration Function"
    "HealthChecksUI__HealthChecks__6__Uri"  = "https://func-migration-mig-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__7__Name" = "Migration: Timeseries API"
    "HealthChecksUI__HealthChecks__7__Uri"  = "https://app-timeseriesapi-mig-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__8__Name" = "eSett: ECP Inbox Function"
    "HealthChecksUI__HealthChecks__8__Uri"  = "https://func-ecp-inbox-esett-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__9__Name" = "eSett: ECP Outbound Function"
    "HealthChecksUI__HealthChecks__9__Uri"  = "https://func-ecp-outbox-esett-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__10__Name" = "eSett: Exchange Event Receiver Function"
    "HealthChecksUI__HealthChecks__10__Uri"  = "https://func-exchange-event-receiver-esett-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__11__Name" = "eSett: Peek Function"
    "HealthChecksUI__HealthChecks__11__Uri"  = "https://func-peek-esett-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__12__Name" = "eSett: Web API"
    "HealthChecksUI__HealthChecks__12__Uri"  = "https://app-webapi-esett-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__13__Name" = "Wholesale: Web API"
    "HealthChecksUI__HealthChecks__13__Uri"  = "https://app-webapi-wholsal-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"


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
