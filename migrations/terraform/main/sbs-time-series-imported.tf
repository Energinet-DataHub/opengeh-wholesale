module "sbtsub_time_series_imported_messages" {
  source             = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v13"
  name               = "sbtsub-${lower(var.domain_name_short)}-imported-time-series-messages"
  topic_id           = azurerm_servicebus_topic.time_series_imported_messages_topic.id
  project_name       = var.domain_name_short
  max_delivery_count = 10
}
