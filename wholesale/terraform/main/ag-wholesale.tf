module "monitor_action_group_wholesale" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=v13"

  name                 = "monitor-group"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name                      = "whl-mon-grp"
  email_receiver_name             = "Wholesale Operations"
  email_receiver_address          = "0cf44ce3.energinet.onmicrosoft.com@emea.teams.ms"
  custom_dimension_subsystem_name = "wholesale"

  query_alerts_list = [
    {
      name        = "exception-trigger"
      query       = <<-QUERY
        exceptions
        | where timestamp > ago(10m)
        | where customDimensions["Subsystem"] in ("wholesale")
        QUERY
      frequency   = 10
      time_window = 60
      operator    = "GreaterThanOrEqual"
      threshold   = 2
    },
  ]
  application_insights_id = data.azurerm_key_vault_secret.appi_shared_id.value
}
