module "sbtsub_wholesale_process_completed_event_listener" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v10"
  name                = local.WHOLESALE_PROCESS_COMPLETED_EVENT_SUBSCRIPTION_NAME
  topic_id            = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_id.id
  project_name        = var.domain_name_short
  max_delivery_count  = 1 
  correlation_filter  = {
    label   = local.WHOLESALE_PROCESS_COMPLETED_EVENT_TYPE_NAME
  }
}