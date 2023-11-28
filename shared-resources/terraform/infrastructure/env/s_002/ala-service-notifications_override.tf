resource "azurerm_monitor_activity_log_alert" "main" {
  name = "ala-shared-servicenotifications-${local.resources_suffix}"
}
