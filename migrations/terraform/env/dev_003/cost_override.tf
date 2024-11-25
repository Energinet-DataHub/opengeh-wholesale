resource "azurerm_consumption_budget_subscription" "budget_alert_default" {
  time_period {
    start_date = "2024-12-01T00:00:00Z"
  }
}
