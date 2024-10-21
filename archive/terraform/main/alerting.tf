module "monitor_action_group" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=monitor-action-group-email_4.0.1"

  name                 = "alerts"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name                 = "arch-alerts"
  email_receiver_name        = "Archive Operations"
  email_receiver_address     = var.alert_email_address

  application_insights_id = data.azurerm_key_vault_secret.appi_shared_id.value
}
