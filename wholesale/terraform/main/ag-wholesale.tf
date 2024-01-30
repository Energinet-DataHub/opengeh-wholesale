module "ag_wholesale" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=v13"

  name                 = "exceptions"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name          = "Wholesale"
  email_receiver_name = "MS Teams Exceptions"
  // In the MS Team channel we must configure the channel to accept emails from outside the team, otherwise it will be blocked.
  email_receiver_address = "0cf44ce3.energinet.onmicrosoft.com@emea.teams.ms"

  query_alerts = {
    query          = <<-QUERY
                  exceptions
                  | where timestamp > ago(10m)
                  | where customDimensions["Subsystem"] in ("wholesale")
                  QUERY
    appinsights_id = data.azurerm_key_vault_secret.appi_shared_id.value
  }
}
