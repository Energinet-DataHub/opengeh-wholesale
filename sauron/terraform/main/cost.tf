resource "azurerm_consumption_budget_subscription" "budget_alert_default" {
  name            = "alert-budget-default-${var.domain_name_short}-${var.environment_short}-${var.environment_instance}"
  subscription_id = data.azurerm_subscription.this.id

  amount     = var.budget_alert_amount
  time_grain = "Monthly"

  time_period {
    # The value of time_rotating.this is known before apply meaning that we won't spam our CD pipeline with changes which we did when using timestamp() function
    start_date = "${formatdate("YYYY", time_rotating.this.rfc3339)}-${formatdate("MM", time_rotating.this.rfc3339)}-01T00:00:00Z"
  }

  filter {
    dimension {
      name = "ResourceGroupName"
      values = [
        azurerm_resource_group.this.name
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

resource "time_rotating" "this" {
  rotation_months = 1
}
