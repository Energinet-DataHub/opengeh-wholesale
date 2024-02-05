module "sbtsub_dh2_bridge_event_listener" {
  source             = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v13"
  name               = "dh2-bridge"
  topic_id           = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_id.value
  project_name       = var.domain_name_short
  max_delivery_count = 10
  sql_filter         = { name = "integration-event-filter", filter = "sys.label = 'GridLossResultProducedV1' or sys.label= 'GridLossResultProducedV1Titans'" }
}
