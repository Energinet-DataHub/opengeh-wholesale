resource "azurerm_consumption_budget_subscription" "budget_alert_default" {
  name            = "alert-budget-default-${var.domain_name_short}-${var.environment_short}-${var.environment_instance}"
  subscription_id = data.azurerm_subscription.this.id

  amount     = 3000
  time_grain = "Monthly"

  time_period {
    start_date = "2024-10-01T00:00:00Z"
  }

  filter {
    dimension {
      name = "ResourceGroupName"
      values = [
        azurerm_resource_group.this.name,
      ]
    }
  }

  notification {
    enabled        = true
    threshold      = 100.0
    operator       = "GreaterThanOrEqualTo"
    threshold_type = "Forecasted"

    contact_emails = [
      "3efe72c1.energinet.onmicrosoft.com@emea.teams.ms"
    ]
  }
}
