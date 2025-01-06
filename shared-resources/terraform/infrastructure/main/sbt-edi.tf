resource "azurerm_servicebus_topic" "edi" {
  name         = "sbt-edi"
  namespace_id = module.sb_domain_relay.id
}

module "kvs_sbt_edi_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sbt-edi-id"
  value        = azurerm_servicebus_topic.edi.id
  key_vault_id = module.kv_shared.id
}

module "kvs_sbt_edi_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sbt-edi-name"
  value        = azurerm_servicebus_topic.edi.name
  key_vault_id = module.kv_shared.id
}
