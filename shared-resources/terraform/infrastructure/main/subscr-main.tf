data "azurerm_subscription" "this" {
  subscription_id = var.subscription_id
}

resource "azurerm_cost_anomaly_alert" "anomaly_alert" {
  name            = "alert-costanomaly-${var.environment_short}-${var.environment_instance}"
  display_name    = "Cost anomaly alert"
  subscription_id = data.azurerm_subscription.this.id
  email_subject   = "Cost Anomaly detected"
  email_addresses = ["3efe72c1.energinet.onmicrosoft.com@emea.teams.ms"]
}
