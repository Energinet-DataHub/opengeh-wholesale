module "app_health_checks_ui" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v13"

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
  ip_restrictions                          = var.ip_restrictions
  scm_ip_restrictions                      = var.ip_restrictions

  # Ensure that IHostedServices are not terminated due to unloading of the application in periods with no traffic
  always_on = true

  app_settings                             = local.default_app_health_checks_ui_app_settings
}

locals {
  default_app_health_checks_ui_app_settings = {
    # Health Checks to monitor
    # Ready - prefix with 0xx
    "HealthChecksUI__HealthChecks__001__Name" = "greenforce-frontend:::Web API"
    "HealthChecksUI__HealthChecks__001__Uri"  = "https://app-bff-fe-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__002__Name" = "geh-market-participant:::Web API"
    "HealthChecksUI__HealthChecks__002__Uri"  = "https://app-webapi-markpart-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__003__Name" = "geh-market-participant:::Function"
    "HealthChecksUI__HealthChecks__003__Uri"  = "https://func-organization-markpart-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__004__Name" = "opengeh-edi:::Web API"
    "HealthChecksUI__HealthChecks__004__Uri"  = "https://func-api-edi-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__005__Name" = "opengeh-migrations:::Dropzoneunzipper Function"
    "HealthChecksUI__HealthChecks__005__Uri"  = "https://func-dropzoneunzipper-mig-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__006__Name" = "opengeh-migrations:::Timeseries Synchronization Function"
    "HealthChecksUI__HealthChecks__006__Uri"  = "https://func-timeseriessynchronization-mig-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__007__Name" = "opengeh-migrations:::Timeseries API"
    "HealthChecksUI__HealthChecks__007__Uri"  = "https://app-timeseriesapi-mig-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__008__Name" = "opengeh-esett-exchange:::ECP Inbox Function"
    "HealthChecksUI__HealthChecks__008__Uri"  = "https://func-ecp-inbox-esett-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__009__Name" = "opengeh-esett-exchange:::ECP Outbound Function"
    "HealthChecksUI__HealthChecks__009__Uri"  = "https://func-ecp-outbox-esett-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__010__Name" = "opengeh-esett-exchange:::Exchange Event Receiver Function"
    "HealthChecksUI__HealthChecks__010__Uri"  = "https://func-exchange-event-receiver-esett-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__011__Name" = "opengeh-esett-exchange:::Peek Function"
    "HealthChecksUI__HealthChecks__011__Uri"  = "https://func-peek-esett-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__012__Name" = "opengeh-esett-exchange:::Web API"
    "HealthChecksUI__HealthChecks__012__Uri"  = "https://app-webapi-esett-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__013__Name" = "opengeh-wholesale:::Web API"
    "HealthChecksUI__HealthChecks__013__Uri"  = "https://app-webapi-wholsal-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__019__Name" = "geh-market-participant:::Certificate Synchronization"
    "HealthChecksUI__HealthChecks__019__Uri"  = "https://func-certificatesynchronization-markpart-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__020__Name" = "dh2-bridge:::Send Grid loss Function"
    "HealthChecksUI__HealthChecks__020__Uri"  = "https://func-grid-loss-sender-dh2brdg-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__021__Name" = "dh2-bridge:::Peek Grid loss Function"
    "HealthChecksUI__HealthChecks__021__Uri"  = "https://func-grid-loss-peek-dh2brdg-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__022__Name" = "dh2-bridge:::Event Receiver Grid loss Function"
    "HealthChecksUI__HealthChecks__022__Uri"  = "https://func-grid-loss-event-receiver-dh2brdg-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__023__Name" = "dh2-bridge:::Grid loss event Simulator"
    "HealthChecksUI__HealthChecks__023__Uri"  = "https://func-grid-loss-simulator-dh2brdg-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__024__Name" = "shared-resources:::func-healthcheck"
    "HealthChecksUI__HealthChecks__024__Uri"  = "https://func-healthchecks-shres-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"

    # Live - prefix with 2xx
    "HealthChecksUI__HealthChecks__200__Name" = "health-checks-ui:::Health Check UI - live"
    "HealthChecksUI__HealthChecks__200__Uri"  = "https://app-healthchecksui-health-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"
    "HealthChecksUI__HealthChecks__201__Name" = "greenforce-frontend:::Web API - live"
    "HealthChecksUI__HealthChecks__201__Uri"  = "https://app-bff-fe-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"
    "HealthChecksUI__HealthChecks__202__Name" = "geh-market-participant:::Web API - live"
    "HealthChecksUI__HealthChecks__202__Uri"  = "https://app-webapi-markpart-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"
    "HealthChecksUI__HealthChecks__203__Name" = "geh-market-participant:::Function - live"
    "HealthChecksUI__HealthChecks__203__Uri"  = "https://func-organization-markpart-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__204__Name" = "opengeh-edi:::Web API - live"
    "HealthChecksUI__HealthChecks__204__Uri"  = "https://func-api-edi-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__205__Name" = "opengeh-migrations:::Dropzoneunzipper Function - live"
    "HealthChecksUI__HealthChecks__205__Uri"  = "https://func-dropzoneunzipper-mig-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__206__Name" = "opengeh-migrations:::Timeseries Synchronization Function - live"
    "HealthChecksUI__HealthChecks__206__Uri"  = "https://func-timeseriessynchronization-mig-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__207__Name" = "opengeh-migrations:::Timeseries API - live"
    "HealthChecksUI__HealthChecks__207__Uri"  = "https://app-timeseriesapi-mig-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"
    "HealthChecksUI__HealthChecks__208__Name" = "opengeh-esett-exchange:::ECP Inbox Function - live"
    "HealthChecksUI__HealthChecks__208__Uri"  = "https://func-ecp-inbox-esett-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__209__Name" = "opengeh-esett-exchange:::ECP Outbound Function - live"
    "HealthChecksUI__HealthChecks__209__Uri"  = "https://func-ecp-outbox-esett-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__210__Name" = "opengeh-esett-exchange:::Exchange Event Receiver Function - live"
    "HealthChecksUI__HealthChecks__210__Uri"  = "https://func-exchange-event-receiver-esett-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__211__Name" = "opengeh-esett-exchange:::Peek Function - live"
    "HealthChecksUI__HealthChecks__211__Uri"  = "https://func-peek-esett-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__212__Name" = "opengeh-esett-exchange:::Web API - live"
    "HealthChecksUI__HealthChecks__212__Uri"  = "https://app-webapi-esett-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"
    "HealthChecksUI__HealthChecks__213__Name" = "opengeh-wholesale:::Web API - live"
    "HealthChecksUI__HealthChecks__213__Uri"  = "https://app-webapi-wholsal-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"
    "HealthChecksUI__HealthChecks__219__Name" = "geh-market-participant:::Certificate Synchronization - live"
    "HealthChecksUI__HealthChecks__219__Uri"  = "https://func-certificatesynchronization-markpart-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__220__Name" = "dh2-bridge:::Send Grid loss Function - live"
    "HealthChecksUI__HealthChecks__220__Uri"  = "https://func-grid-loss-sender-dh2brdg-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__221__Name" = "dh2-bridge:::Peek Grid loss Function - live"
    "HealthChecksUI__HealthChecks__221__Uri"  = "https://func-grid-loss-peek-dh2brdg-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__222__Name" = "dh2-bridge:::Event Receiver Grid loss Function - live"
    "HealthChecksUI__HealthChecks__222__Uri"  = "https://func-grid-loss-event-receiver-dh2brdg-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__223__Name" = "dh2-bridge:::Grid loss event Simulator - live"
    "HealthChecksUI__HealthChecks__223__Uri"  = "https://func-grid-loss-simulator-dh2brdg-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__224__Name" = "shared-resources:::func-healthcheck - live"
    "HealthChecksUI__HealthChecks__224__Uri"  = "https://func-healthchecks-shres-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"

    # Polling Interval
    "HealthChecksUI__EvaluationTimeinSeconds" = 60
    # Max. health status history entries returned to UI
    "HealthChecksUI__MaximumExecutionHistoriesPerEndpoint" = 30

    # Logging
    "Logging__LogLevel__Default"              = "Information"
    "Logging__LogLevel__Microsoft.AspNetCore" = "Warning"

    # Tab Title
    "TAB_TITLE" = "Health Checks UI - ${var.environment_short}-we-${var.environment_instance}"
  }
}
