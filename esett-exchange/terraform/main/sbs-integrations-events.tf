module "sbtsub_esett_exchange_event_listener" {
  source             = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v12"
  name               = "esett-exchange"
  topic_id           = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_id.value
  project_name       = var.domain_name_short
  max_delivery_count = 10
  sql_filter         = { name = "integration-event-filter", filter = "sys.label = 'GridAreaOwnershipAssigned' or sys.label= 'EnergyResultProducedV1Extended'" }
}
