resource "azurerm_servicebus_topic" "brs021forwardmetereddatanotify" {
  name         = "sbt-brs021forwardmetereddatanotify"
  namespace_id = module.sb_domain_relay.id
}

module "kvs_sbt_brs021forwardmetereddatanotify_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sbt-brs021forwardmetereddatanotify-id"
  value        = azurerm_servicebus_topic.brs021forwardmetereddatanotify.id
  key_vault_id = module.kv_shared.id
}

module "kvs_sbt_brs021forwardmetereddatanotify_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sbt-brs021forwardmetereddatanotify-name"
  value        = azurerm_servicebus_topic.brs021forwardmetereddatanotify.name
  key_vault_id = module.kv_shared.id
}
