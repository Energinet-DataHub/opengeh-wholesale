resource "azurerm_servicebus_topic" "processmanagernotify" {
  name         = "sbt-processmanagernotify"
  namespace_id = module.sb_domain_relay.id
}

module "kvs_sbt_processmanagernotify_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sbt-processmanagernotify-id"
  value        = azurerm_servicebus_topic.processmanagernotify.id
  key_vault_id = module.kv_shared.id
}

module "kvs_sbt_processmanagernotify_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sbt-processmanagernotify-name"
  value        = azurerm_servicebus_topic.processmanagernotify.name
  key_vault_id = module.kv_shared.id
}
