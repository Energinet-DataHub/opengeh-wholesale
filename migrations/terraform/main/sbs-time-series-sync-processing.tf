﻿module "sbtsub_time_series_sync_processing" {
  source             = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=14.22.0"
  name               = "sbtsub-${lower(var.domain_name_short)}-time-series-sync-processing"
  topic_id           = azurerm_servicebus_topic.time_series_imported_messages_topic.id
  project_name       = var.domain_name_short
  max_delivery_count = 10
}
