locals {
  email_receiver_name = "Wholesale Operations"

  // In the MS Team channel we must configure the channel to accept emails from outside the team, otherwise it will be blocked.
  email_receiver_address = "0cf44ce3.energinet.onmicrosoft.com@emea.teams.ms"
}

module "ag_wholesale_exceptions" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=v13"

  name                 = "exceptions"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name             = "Exceptions"
  email_receiver_name    = local.email_receiver_name
  email_receiver_address = local.email_receiver_address

  query_alerts = {
    query          = <<-QUERY
                  exceptions
                  | where timestamp > ago(10m)
                  | where customDimensions["Subsystem"] in ("wholesale")
                  QUERY
    appinsights_id = data.azurerm_key_vault_secret.appi_shared_id.value
  }
}

# Alert when HTTP request errors occur more than 5 times per hour for 8 hours in a row.
# The purpose is to try to detect if something has been automated, but is not working.
# An experienced example was always-on resulting in frequent requests generating 404 responses.
# This is _not_ related to security.
module "ag_wholesale_request_errors" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=v13"

  name                 = "request-errors"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name             = "Req Errors"
  email_receiver_name    = local.email_receiver_name
  email_receiver_address = local.email_receiver_address

  query_alerts = {
    query          = <<-QUERY
let startTime = ago(1d);
let endTime = now();
let timeSeriesResolution = 1h; // Resolution of the time series
requests
| where customDimensions["Subsystem"] in ("wholesale")
| where timestamp between(startTime .. endTime)
| summarize failCount = sum(iif(success == "False", 1, 0)) by timeSlot = bin(timestamp, timeSeriesResolution)
| where failCount >= 5 // Filtering hours with 5 or more failed requests
| summarize count() by bin(timeSlot, 8h) // Aggregating into 8-hour windows
| where count_ >= 8 // Checking for windows with sufficient failed request counts
| project timeWindowStart = timeSlot, countInWindow = count_
                  QUERY
    appinsights_id = data.azurerm_key_vault_secret.appi_shared_id.value
  }
}
