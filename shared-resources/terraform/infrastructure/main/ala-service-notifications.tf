resource "azurerm_monitor_activity_log_alert" "main" {
  name                = "ala-shared-servicenotifications-${local.resources_suffix}"
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
    action_group_id = module.ag_primary.id
  }

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }
}
