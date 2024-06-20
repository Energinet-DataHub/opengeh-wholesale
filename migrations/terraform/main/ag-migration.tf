module "monitor_action_group_mig" {
  count = var.alert_email_address != null ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=14.22.0"

  name                 = "alerts"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name                      = "mig-mon-grp"
  email_receiver_name             = "Migration Operations"
  email_receiver_address          = var.alert_email_address
  custom_dimension_subsystem_name = "migration"

  application_insights_id = data.azurerm_key_vault_secret.appi_id.value
}
