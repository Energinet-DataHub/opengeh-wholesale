resource "azurerm_eventhub" "submitted_transactions_notifications" {
  name                = "evh-submitted-transactions-notification-${local.resources_suffix}"
  namespace_name      = module.evhns_subsystemrelay.name
  resource_group_name = azurerm_resource_group.this.name
  partition_count     = 1
  message_retention   = 7
}

module "kvs_evh_submitted_transactions_notifications_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "evh-submitted-transactions-notifications-id"
  value        = azurerm_eventhub.submitted_transactions_notifications.id
  key_vault_id = module.kv_shared.id
}

module "kvs_evh_submitted_transactions_notifications_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "evh-submitted-transactions-notifications-name"
  value        = azurerm_eventhub.submitted_transactions_notifications.name
  key_vault_id = module.kv_shared.id
}
