module "monitor_action_group" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=v13"

  name                 = "monitor-group"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name                      = "mig-mon-grp"
  email_receiver_name             = "Migration Operations"
  email_receiver_address          = "36217b88.energinet.onmicrosoft.com@emea.teams.ms"
  custom_dimension_subsystem_name = "migration"

  query_alerts_list       = []
  application_insights_id = data.azurerm_key_vault_secret.appi_id.value
}
