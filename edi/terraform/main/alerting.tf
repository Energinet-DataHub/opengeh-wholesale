module "monitor_action_group_edi" {
  count  = var.alert_email_address != null ? 1 : 0
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=monitor-action-group-email_7.0.0"

  name                 = "alerts"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name                 = "edi-alerts"
  email_receiver_name        = "Alerts-Edi-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  email_receiver_address     = var.alert_email_address
  custom_dimension_subsystem = ["EDI"]
  application_insights_id    = data.azurerm_key_vault_secret.appi_shared_id.value

  default_query_exceptions_errors = {
    enabled = false
  }

  query_alerts_list = [
    {
      name        = "exception-trigger"
      description = "Alert when an exception occurs"
      query       = <<-QUERY
                      exceptions
                        | where timestamp > ago(10m)
                        | where customDimensions["Subsystem"] == "EDI"
                        // avoid triggering alert when exception is logged as a warning or lower
                        | where severityLevel >= 3
                        // avoid triggering alert when exception is logged by health check
                        | where customDimensions["EventName"] != "HealthCheckEnd"
                        | where (type == "System.Threading.Tasks.TaskCanceledException" and customDimensions["CategoryName"] == "Microsoft.AspNetCore.Server.Kestrel") == false
                        | where (type == "System.Threading.Tasks.TaskCanceledException" and customDimensions["AzureFunctions_FunctionName"] == "HealthCheck") == false
                    QUERY
      severity    = 1
      frequency   = 5
      time_window = 5
      threshold   = 0
      operator    = "GreaterThan"
    },
  ]
}
