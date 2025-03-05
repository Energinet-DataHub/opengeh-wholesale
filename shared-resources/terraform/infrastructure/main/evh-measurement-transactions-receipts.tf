resource "azurerm_eventhub" "measurement_transactions_receipts" {
  name                = "evh-measurement-transactions-receipts-${local.resources_suffix}"
  namespace_name      = module.evhns_subsystemrelay.name
  resource_group_name = azurerm_resource_group.this.name
  partition_count     = 1
  message_retention   = 7
}

module "kvs_evh_measurement_transactions_receipts_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "evh-measurement-transactions-receipts-id"
  value        = azurerm_eventhub.measurement_transactions_receipts.id
  key_vault_id = module.kv_shared.id
}

module "kvs_evh_measurement_transactions_receipts_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "evh-measurement-transactions-receipts-name"
  value        = azurerm_eventhub.measurement_transactions_receipts.name
  key_vault_id = module.kv_shared.id
}
