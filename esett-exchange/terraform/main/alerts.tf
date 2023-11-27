resource "azurerm_monitor_action_group" "esett_exchange" {
  name                = "ag-${local.name_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  short_name          = "ag-esett"

  email_receiver {
    name                    = "alerts-eSettExchange-${local.name_suffix}"
    email_address           = "it-dh-support@energinet.dk"
    use_common_alert_schema = true
  }
}

resource "azurerm_monitor_scheduled_query_rules_alert_v2" "esett_exchange_deadline_alert" {
  # NOTE: The name prefix 'alert-eSettExchangeDeadline' is used by ServiceNow to detect the alert and create an incident.
  name                    = "alert-eSettExchangeDeadline-${local.name_suffix}"
  location                = azurerm_resource_group.this.location
  resource_group_name     = var.shared_resources_resource_group_name
  auto_mitigation_enabled = true

  evaluation_frequency = "PT10M"
  window_duration      = "PT10M"
  scopes               = [data.azurerm_key_vault_secret.appi_shared_id.value]
  severity             = 1

  action {
    action_groups = [azurerm_monitor_action_group.esett_exchange.id]
  }

  criteria {
    query = <<-QUERY
       let nowInDk = datetime_utc_to_local(now(), 'Europe/Copenhagen');
       let dkStartOfDayInUtc = datetime_local_to_utc(startofday(nowInDk), 'Europe/Copenhagen');
       let isPastThresholdTime = hourofday(nowInDk) >= 7;
       traces
       | where
           timestamp >= dkStartOfDayInUtc and
           timestamp < dkStartOfDayInUtc + 7h
       | where
           operation_Name == 'EcpOutboxTrigger' and
           customDimensions.EventName == 'eSett Deadline Alert Tag'
       | count
       | project
         MessageCount = iff(isPastThresholdTime, Count, 1)
       QUERY

    time_aggregation_method = "Minimum"
    metric_measure_column   = "MessageCount"
    operator                = "LessThan"
    threshold               = 1
  }

  description    = "eSett Exchange alert for when no messages are sent before daily deadline."
  enabled        = false
}
