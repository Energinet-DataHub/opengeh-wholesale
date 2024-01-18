resource "azurerm_monitor_action_group" "this" {
  count = 0
}

resource "azurerm_monitor_scheduled_query_rules_alert" "this" {
  count = 0
}
