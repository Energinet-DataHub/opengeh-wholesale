module "sbtsub_time_series_sync_audit" {
  source             = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=service-bus-topic-subscription_5.0.0"
  name               = "sbtsub-${lower(var.domain_name_short)}-time-series-sync-audit"
  topic_id           = azurerm_servicebus_topic.time_series_imported_messages_topic.id
  project_name       = var.domain_name_short
  max_delivery_count = 10
}
