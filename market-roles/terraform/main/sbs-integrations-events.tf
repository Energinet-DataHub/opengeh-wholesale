module "sbt_domain_events" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic?ref=v10"
  name                = "integration-events"
  project_name        = var.domain_name_short
  namespace_id        = data.azurerm_key_vault_secret.sb_integration_events_id.value
}

module "sbtsub_wholesale_process_completed_event_listener" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v10"
  name                = local.WHOLESALE_PROCESS_COMPLETED_EVENT_SUBSCRIPTION_NAME
  topic_id            = module.sbt_domain_events.id
  project_name        = var.domain_name_short
  max_delivery_count  = 1 
  correlation_filter  = {
    label   = local.WHOLESALE_PROCESS_COMPLETED_EVENT_TYPE_NAME
  }
}