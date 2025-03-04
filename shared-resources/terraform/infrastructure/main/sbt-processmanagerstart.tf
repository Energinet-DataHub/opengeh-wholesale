resource "azurerm_servicebus_topic" "processmanagerstart" {
  name         = "sbt-processmanagerstart"
  namespace_id = module.sb_domain_relay.id
}

module "kvs_sbt_processmanagerstart_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sbt-processmanagerstart-id"
  value        = azurerm_servicebus_topic.processmanagerstart.id
  key_vault_id = module.kv_shared.id
}

module "kvs_sbt_processmanagerstart_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sbt-processmanagerstart-name"
  value        = azurerm_servicebus_topic.processmanagerstart.name
  key_vault_id = module.kv_shared.id
}
