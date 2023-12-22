module "sbtsub_edi_integration_event_listener" {
  source             = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v13"
  name               = local.INTEGRATION_EVENTS_SUBSCRIPTION_NAME
  topic_id           = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_id.value
  project_name       = var.domain_name_short
  max_delivery_count = 10
}
