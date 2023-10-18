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
    # Ready
    "HealthChecksUI__HealthChecks__0__Name"  = "Health Check UI"
    "HealthChecksUI__HealthChecks__0__Uri"   = "https://app-healthchecksui-health-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__1__Name"  = "BFF: Web API"
    "HealthChecksUI__HealthChecks__1__Uri"   = "https://app-bff-fe-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__2__Name"  = "Market Participant: Web API"
    "HealthChecksUI__HealthChecks__2__Uri"   = "https://app-webapi-markpart-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__3__Name"  = "Market Participant: Function"
    "HealthChecksUI__HealthChecks__3__Uri"   = "https://func-organization-markpart-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__4__Name"  = "EDI: Function"
    "HealthChecksUI__HealthChecks__4__Uri"   = "https://func-api-edi-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__5__Name"  = "Migration: Dropzoneunzipper Function"
    "HealthChecksUI__HealthChecks__5__Uri"   = "https://func-dropzoneunzipper-mig-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__6__Name"  = "Migration: Timeseries Synchronization Function"
    "HealthChecksUI__HealthChecks__6__Uri"   = "https://func-timeseriessynchronization-mig-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__7__Name"  = "Migration: Timeseries API"
    "HealthChecksUI__HealthChecks__7__Uri"   = "https://app-timeseriesapi-mig-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__8__Name"  = "eSett: ECP Inbox Function"
    "HealthChecksUI__HealthChecks__8__Uri"   = "https://func-ecp-inbox-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__9__Name"  = "eSett: ECP Outbound Function"
    "HealthChecksUI__HealthChecks__9__Uri"   = "https://func-ecp-outbox-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__10__Name" = "eSett: Exchange Event Receiver Function"
    "HealthChecksUI__HealthChecks__10__Uri"  = "https://func-exchange-event-receiver-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__11__Name" = "eSett: Peek Function"
    "HealthChecksUI__HealthChecks__11__Uri"  = "https://func-peek-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__12__Name" = "eSett: Web API"
    "HealthChecksUI__HealthChecks__12__Uri"  = "https://app-webapi-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__13__Name" = "Wholesale: Web API"
    "HealthChecksUI__HealthChecks__13__Uri"  = "https://app-webapi-wholsal-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    # Live
    "HealthChecksUI__HealthChecks__14__Name" = "Health Check UI - live"
    "HealthChecksUI__HealthChecks__14__Uri"  = "https://app-healthchecksui-health-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"
    "HealthChecksUI__HealthChecks__15__Name" = "BFF: Web API - live"
    "HealthChecksUI__HealthChecks__15__Uri"  = "https://app-bff-fe-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"
    "HealthChecksUI__HealthChecks__16__Name" = "Market Participant: Web API - live"
    "HealthChecksUI__HealthChecks__16__Uri"  = "https://app-webapi-markpart-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"
    "HealthChecksUI__HealthChecks__17__Name" = "Market Participant: Function - live"
    "HealthChecksUI__HealthChecks__17__Uri"  = "https://func-organization-markpart-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__18__Name" = "EDI: Function - live"
    "HealthChecksUI__HealthChecks__18__Uri"  = "https://func-api-edi-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__19__Name" = "Migration: Dropzoneunzipper Function - live"
    "HealthChecksUI__HealthChecks__19__Uri"  = "https://func-dropzoneunzipper-mig-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__20__Name" = "Migration: Timeseries Synchronization Function - live"
    "HealthChecksUI__HealthChecks__20__Uri"  = "https://func-timeseriessynchronization-mig-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__21__Name" = "Migration: Timeseries API - live"
    "HealthChecksUI__HealthChecks__21__Uri"  = "https://app-timeseriesapi-mig-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"
    "HealthChecksUI__HealthChecks__22__Name" = "eSett: ECP Inbox Function - live"
    "HealthChecksUI__HealthChecks__22__Uri"  = "https://func-ecp-inbox-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__23__Name" = "eSett: ECP Outbound Function - live"
    "HealthChecksUI__HealthChecks__23__Uri"  = "https://func-ecp-outbox-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__24__Name" = "eSett: Exchange Event Receiver Function - live"
    "HealthChecksUI__HealthChecks__24__Uri"  = "https://func-exchange-event-receiver-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__25__Name" = "eSett: Peek Function - live"
    "HealthChecksUI__HealthChecks__25__Uri"  = "https://func-peek-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__26__Name" = "eSett: Web API - live"
    "HealthChecksUI__HealthChecks__26__Uri"  = "https://app-webapi-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"
    "HealthChecksUI__HealthChecks__27__Name" = "Wholesale: Web API - live"
    "HealthChecksUI__HealthChecks__27__Uri"  = "https://app-webapi-wholsal-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"

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

    # Tab Title
    "TAB_TITLE" = "Health Checks UI - ${var.environment_short}-${var.environment_instance}"
  }
}
