module "monitor_action_group_wholesale" {
  count  = var.alert_email_address != null ? 1 : 0
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=monitor-action-group-email_6.0.1"

  name                 = "alerts"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name                 = "whl-alerts"
  email_receiver_name        = "Wholesale Operations"
  email_receiver_address     = var.alert_email_address
  custom_dimension_subsystem = ["wholesale"]

  application_insights_id = data.azurerm_key_vault_secret.appi_shared_id.value

  default_query_exceptions_errors = {
    enabled = false
  }
  default_query_request_errors = {
    enabled = false
  }

  query_alerts_list = [
    //
    // Wholesale .NET alerts
    //
    {
      name        = "request-errors-dotnet"
      description = "Default alert for request errors."
      query       = <<-QUERY
                      let startTime = ago(1d);
                      let endTime = now();
                      let resolution = 1h;
                      requests
                      | where timestamp between(startTime .. endTime)
                      | where customDimensions["Subsystem"] == "wholesale-dotnet"
                      | summarize failCount = sum(iif(success == "False", 1, 0)) by timeSlot = bin(timestamp, resolution)
                      | where failCount >= 5 // Filtering hours with 5 or more failed requests
                      | summarize count() by bin(timeSlot, 8h) // Aggregating into 8-hour windows
                      | where count_ >= 8 // Checking for windows with sufficient failed request counts
                      | project timeWindowStart = timeSlot, countInWindow = count_
                      QUERY
      severity    = 1
      frequency   = 5
      time_window = 5
      threshold   = 0
      operator    = "GreaterThan"
    },
    {
      name        = "exception-trigger-dotnet"
      description = "Alert when total results cross threshold"
      query       = <<-QUERY
                      exceptions
                        | where timestamp > ago(10m)
                        | where customDimensions["Subsystem"] == "wholesale-dotnet"
                        // avoid triggering alert when exception is logged as a warning or lower
                        | where severityLevel >= 3
                        // avoid triggering alert when exception is logged by health check
                        | where customDimensions["EventName"] != "HealthCheckEnd"
                    QUERY
      severity    = 1
      frequency   = 5
      time_window = 5
      threshold   = 0
      operator    = "GreaterThan"
    },
  ]
}
