module "sbtsub_wholesale_process_completed_event_listener" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v10"
  name                = "wholesale_process_completed_event_listener"
  topic_id            = data.azurerm_key_vault_secret.sbt_sharedres_integrationevent_received_id.value
  project_name        = var.domain_name_short
  max_delivery_count  = 1 
  correlation_filter  = {
    label   = "ProcessCompleted"
  }
}