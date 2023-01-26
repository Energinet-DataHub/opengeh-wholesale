module "sbt_domain_events" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic?ref=v10"
  name                = "domain-events"
  project_name        = var.domain_name_short
  namespace_id        = data.azurerm_key_vault_secret.sb_integration_events_id.value
}

module "sbtsub_create_settlement_reports_when_batch_completed" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v10"
  name                = local.CREATE_SETTLEMENT_REPORTS_WHEN_COMPLETED_BATCH_SUBSCRIPTION_NAME
  project_name        = var.domain_name_short
  topic_id            = module.sbt_domain_events.id
  max_delivery_count  = 10
  correlation_filter  = {
    label = local.BATCH_COMPLETED_EVENT_NAME
  }
}

module "sbtsub_publish_process_completed_when_batch_completed" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v10"
  name                = local.PUBLISH_PROCESSES_COMPLETED_WHEN_COMPLETED_BATCH_SUBSCRIPTION_NAME
  project_name        = var.domain_name_short
  topic_id            = module.sbt_domain_events.id
  max_delivery_count  = 10
  correlation_filter  = {
    label = local.BATCH_COMPLETED_EVENT_NAME
  }
}

module "sbtsub_publish_processescompletedintegrationevent_when_processcompleted" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v10"
  name                = local.PUBLISH_PROCESSESCOMPLETEDINTEGRATIONEVENT_WHEN_PROCESSCOMPLETED_SUBSCRIPTION_NAME
  project_name        = var.domain_name_short
  topic_id            = module.sbt_domain_events.id
  max_delivery_count  = 10
  correlation_filter  = {
    label = local.PROCESS_COMPLETED_EVENT_NAME
  }
}
