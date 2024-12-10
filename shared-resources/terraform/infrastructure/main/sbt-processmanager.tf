resource "azurerm_servicebus_topic" "processmanager" {
  name         = "sbt-processmanager"
  namespace_id = module.sb_domain_relay.id
}

module "kvs_sbt_processmanager_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sbt-processmanager-id"
  value        = azurerm_servicebus_topic.processmanager.id
  key_vault_id = module.kv_shared.id
}

module "kvs_sbt_processmanager_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sbt-processmanager-name"
  value        = azurerm_servicebus_topic.processmanager.name
  key_vault_id = module.kv_shared.id
}
