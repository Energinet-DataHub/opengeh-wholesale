resource "azurerm_eventhub" "submitted_transactions" {
  name                = "evh-submitted-transactions-${local.resources_suffix}"
  namespace_name      = module.evhns_subsystemrelay.name
  resource_group_name = azurerm_resource_group.this.name
  partition_count     = 1
  message_retention   = 7
}

module "kvs_evh_submitted_transactions_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "evh-submitted-transactions-id"
  value        = azurerm_eventhub.submitted_transactions.id
  key_vault_id = module.kv_shared.id
}

module "kvs_evh_submitted_transactions_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "evh-submitted-transactions-name"
  value        = azurerm_eventhub.submitted_transactions.name
  key_vault_id = module.kv_shared.id
}
