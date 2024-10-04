module "monitor_action_group_azuremaintenance" {
  count = var.azure_maintenance_alerts_email_address != null ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=monitor-action-group-email_5.0.0"

  name                 = "azure-maintenance"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name             = "azure-alerts"
  email_receiver_name    = "Azure incidents and planned maintenance"
  email_receiver_address = var.azure_maintenance_alerts_email_address
}

resource "azurerm_monitor_activity_log_alert" "main" {
  count = var.azure_maintenance_alerts_email_address != null ? 1 : 0

  name                = "ala-shared-servicenotifications-${local.resources_suffix}"
  location            = "Global"
  resource_group_name = azurerm_resource_group.this.name

  scopes = [
    data.azurerm_subscription.this.id
  ]
  description = "Alert will fire in case of a service issue or planned maintenance of any resources in the subscription."

  criteria {
    category = "ServiceHealth"

    service_health {
      events = [
        "Incident",
        "Maintenance"
      ]
    }
  }

  action {
    action_group_id = module.monitor_action_group_azuremaintenance[0].id
  }

  tags = local.tags
}
