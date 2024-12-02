resource "azurerm_monitor_alert_processing_rule_action_group" "backupvaults" {
  name                 = "apr-backupvaults-${local.resources_suffix}"
  resource_group_name  = azurerm_resource_group.this.name
  scopes               = [data.azurerm_subscription.this.id]
  add_action_group_ids = [module.monitor_action_group_platform.id]

  condition {
    target_resource_type {
      operator = "Equals"
      // These are the values when generating a template through "automation -> export template" in the azure portal
      // it would seem there is multiple ways of naming the same resource type
      values = ["Microsoft.DataProtection/backupVaults", "microsoft.dataprotection/backupvaults"]
    }
  }

  tags = local.tags
}
