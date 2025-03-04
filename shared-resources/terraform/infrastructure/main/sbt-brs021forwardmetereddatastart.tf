resource "azurerm_servicebus_topic" "brs021forwardmetereddatastart" {
  name         = "sbt-brs021forwardmetereddatastart"
  namespace_id = module.sb_domain_relay.id
}

module "kvs_sbt_brs021forwardmetereddatastart_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sbt-brs021forwardmetereddatastart-id"
  value        = azurerm_servicebus_topic.brs021forwardmetereddatastart.id
  key_vault_id = module.kv_shared.id
}

module "kvs_sbt_brs021forwardmetereddatastart_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sbt-brs021forwardmetereddatastart-name"
  value        = azurerm_servicebus_topic.brs021forwardmetereddatastart.name
  key_vault_id = module.kv_shared.id
}
