resource "azurerm_monitor_metric_alert" "dropzoneunzipper_mp_metric_alert" {
  name                = "alert-dropzoneunzipper-not-received-mp-metric-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  scopes              = [azurerm_eventgrid_system_topic.st_dh2data.id]
  description         = "Check every 1 hour if there has been any MP data received within the last 24 hours, alert if not"
  severity            = 1
  enabled             = true

  criteria {
    metric_namespace = "Microsoft.EventGrid/systemTopics"
    metric_name      = "DeliverySuccessCount"
    aggregation      = "Count"
    operator         = "LessThanOrEqual"
    threshold        = 0
    dimension {
      name     = "EventSubscriptionName"
      operator = "Include"
      values   = [azurerm_eventgrid_system_topic_event_subscription.dh2_metering_point_history.name]
    }
  }

  action {
    action_group_id = module.monitor_action_group_mig[0].id
  }

  frequency   = "PT1H"
  window_size = "P1D"
}

resource "azurerm_monitor_metric_alert" "dropzoneunzipper_ch_metric_alert" {
  name                = "alert-dropzoneunzipper-not-received-ch-metric-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  scopes              = [azurerm_eventgrid_system_topic.st_dh2data.id]
  description         = "Check every 1 hour if there has been any CH data received within the last 24 hours, alert if not"
  severity            = 1
  enabled             = true

  criteria {
    metric_namespace = "Microsoft.EventGrid/systemTopics"
    metric_name      = "DeliverySuccessCount"
    aggregation      = "Count"
    operator         = "LessThanOrEqual"
    threshold        = 0
    dimension {
      name     = "EventSubscriptionName"
      operator = "Include"
      values   = [azurerm_eventgrid_system_topic_event_subscription.dh2data_charges.name]
    }
  }

  action {
    action_group_id = module.monitor_action_group_mig[0].id
  }

  frequency   = "PT1H"
  window_size = "P1D"
}

resource "azurerm_monitor_metric_alert" "timeseriessynchronization_ts_metric_alert" {
  name                = "alert-timeseriessynchronization-ts-sync-not-received-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  scopes              = [azurerm_eventgrid_system_topic.st_dh2data.id]
  description         = "Check every 30 minutes if there has been any sync ts data received within the last 30 minutes, alert if not"
  severity            = 1
  enabled             = true

  criteria {
    metric_namespace = "Microsoft.EventGrid/systemTopics"
    metric_name      = "DeliverySuccessCount"
    aggregation      = "Count"
    operator         = "LessThanOrEqual"
    threshold        = 0
    dimension {
      name     = "EventSubscriptionName"
      operator = "Include"
      values   = [azurerm_eventgrid_system_topic_event_subscription.dh2_timeseries_synchronization.name]
    }
  }

  action {
    action_group_id = module.monitor_action_group_mig[0].id
  }

  frequency   = "PT30M"
  window_size = "PT30M"
}

resource "azurerm_monitor_metric_alert" "timeseriessynchronization_deadletter_queue_metric_alert" {
  name                = "alert-timeseriessynchronization-deadletter-queue-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  scopes              = [data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value]
  description         = "Check deadletter queue, ensure it is empty every hour ensure it is empty"
  severity            = 1
  enabled             = true

  criteria {
    metric_namespace = "Microsoft.ServiceBus/namespaces"
    metric_name      = "DeadletteredMessages"
    aggregation      = "Maximum"
    operator         = "GreaterThan"
    threshold        = 0
    dimension {
      name     = "EntityName"
      operator = "Include"
      values   = [module.sbtsub_time_series_sync_processing.name]
    }
  }

  action {
    action_group_id = module.monitor_action_group_mig[0].id
  }

  frequency   = "PT1H"
  window_size = "PT1H"
}
