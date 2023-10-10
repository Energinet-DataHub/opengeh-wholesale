resource "azurerm_servicebus_topic" "domainrelay_integrationevent_received" {
  name                         = "sbt-${lower(var.domain_name_short)}-integrationevent-received"
  namespace_id                 = module.sb_domain_relay.id
}

module "kvs_sbt_domainrelay_integrationevent_received_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name         = "sbt-shres-integrationevent-received-id"
  value        = azurerm_servicebus_topic.domainrelay_integrationevent_received.id
  key_vault_id = module.kv_shared.id
}

module "kvs_sbt_domainrelay_integrationevent_received_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name         = "sbt-shres-integrationevent-received-name"
  value        = azurerm_servicebus_topic.domainrelay_integrationevent_received.name
  key_vault_id = module.kv_shared.id
}
