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
    # Ready - prefix with 0xx
    "HealthChecksUI__HealthChecks__000__Name"  = "Health Check UI"
    "HealthChecksUI__HealthChecks__000__Uri"   = "https://app-healthchecksui-health-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__001__Name"  = "BFF: Web API"
    "HealthChecksUI__HealthChecks__001__Uri"   = "https://app-bff-fe-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__002__Name"  = "Market Participant: Web API"
    "HealthChecksUI__HealthChecks__002__Uri"   = "https://app-webapi-markpart-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__003__Name"  = "Market Participant: Function"
    "HealthChecksUI__HealthChecks__003__Uri"   = "https://func-organization-markpart-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__004__Name"  = "EDI: Function"
    "HealthChecksUI__HealthChecks__004__Uri"   = "https://func-api-edi-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__005__Name"  = "Migration: Dropzoneunzipper Function"
    "HealthChecksUI__HealthChecks__005__Uri"   = "https://func-dropzoneunzipper-mig-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__006__Name"  = "Migration: Timeseries Synchronization Function"
    "HealthChecksUI__HealthChecks__006__Uri"   = "https://func-timeseriessynchronization-mig-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__007__Name"  = "Migration: Timeseries API"
    "HealthChecksUI__HealthChecks__007__Uri"   = "https://app-timeseriesapi-mig-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__008__Name"  = "eSett: ECP Inbox Function"
    "HealthChecksUI__HealthChecks__008__Uri"   = "https://func-ecp-inbox-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__009__Name"  = "eSett: ECP Outbound Function"
    "HealthChecksUI__HealthChecks__009__Uri"   = "https://func-ecp-outbox-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__010__Name" = "eSett: Exchange Event Receiver Function"
    "HealthChecksUI__HealthChecks__010__Uri"  = "https://func-exchange-event-receiver-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__011__Name" = "eSett: Peek Function"
    "HealthChecksUI__HealthChecks__011__Uri"  = "https://func-peek-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__012__Name" = "eSett: Web API"
    "HealthChecksUI__HealthChecks__012__Uri"  = "https://app-webapi-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    "HealthChecksUI__HealthChecks__013__Name" = "Wholesale: Web API"
    "HealthChecksUI__HealthChecks__013__Uri"  = "https://app-webapi-wholsal-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/ready"
    # Live - prefix with 2xx
    "HealthChecksUI__HealthChecks__200__Name" = "Health Check UI - live"
    "HealthChecksUI__HealthChecks__200__Uri"  = "https://app-healthchecksui-health-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"
    "HealthChecksUI__HealthChecks__201__Name" = "BFF: Web API - live"
    "HealthChecksUI__HealthChecks__201__Uri"  = "https://app-bff-fe-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"
    "HealthChecksUI__HealthChecks__202__Name" = "Market Participant: Web API - live"
    "HealthChecksUI__HealthChecks__202__Uri"  = "https://app-webapi-markpart-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"
    "HealthChecksUI__HealthChecks__203__Name" = "Market Participant: Function - live"
    "HealthChecksUI__HealthChecks__203__Uri"  = "https://func-organization-markpart-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__204__Name" = "EDI: Function - live"
    "HealthChecksUI__HealthChecks__204__Uri"  = "https://func-api-edi-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__205__Name" = "Migration: Dropzoneunzipper Function - live"
    "HealthChecksUI__HealthChecks__205__Uri"  = "https://func-dropzoneunzipper-mig-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__206__Name" = "Migration: Timeseries Synchronization Function - live"
    "HealthChecksUI__HealthChecks__206__Uri"  = "https://func-timeseriessynchronization-mig-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__207__Name" = "Migration: Timeseries API - live"
    "HealthChecksUI__HealthChecks__207__Uri"  = "https://app-timeseriesapi-mig-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"
    "HealthChecksUI__HealthChecks__208__Name" = "eSett: ECP Inbox Function - live"
    "HealthChecksUI__HealthChecks__208__Uri"  = "https://func-ecp-inbox-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__209__Name" = "eSett: ECP Outbound Function - live"
    "HealthChecksUI__HealthChecks__209__Uri"  = "https://func-ecp-outbox-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__210__Name" = "eSett: Exchange Event Receiver Function - live"
    "HealthChecksUI__HealthChecks__210__Uri"  = "https://func-exchange-event-receiver-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__211__Name" = "eSett: Peek Function - live"
    "HealthChecksUI__HealthChecks__211__Uri"  = "https://func-peek-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__212__Name" = "eSett: Web API - live"
    "HealthChecksUI__HealthChecks__212__Uri"  = "https://app-webapi-esett-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"
    "HealthChecksUI__HealthChecks__213__Name" = "Wholesale: Web API - live"
    "HealthChecksUI__HealthChecks__213__Uri"  = "https://app-webapi-wholsal-${lower(var.environment_short)}-${lower(var.environment_instance)}.azurewebsites.net/monitor/live"

    # Polling Interval
    "HealthChecksUI__EvaluationTimeinSeconds" = 60
    # Max. health status history entries returned to UI
    "HealthChecksUI__MaximumExecutionHistoriesPerEndpoint" = 30

    # Logging
    "Logging__LogLevel__Default"              = "Information"
    "Logging__LogLevel__Microsoft.AspNetCore" = "Warning"

    # Tab Title
    "TAB_TITLE" = "Health Checks UI - ${var.environment_short}-${var.environment_instance}"
  }
}
