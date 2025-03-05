resource "azurerm_eventhub" "evh_brs021forwardmetereddatanotify" {
  name                = "evh-brs021forwardmetereddatanotify-${local.resources_suffix}"
  namespace_name      = module.evhns_subsystemrelay.name
  resource_group_name = azurerm_resource_group.this.name
  partition_count     = 1
  message_retention   = 7
}

module "kvs_evh_brs021forwardmetereddatanotify_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "evh-brs021forwardmetereddatanotify-id"
  value        = azurerm_eventhub.evh_brs021forwardmetereddatanotify.id
  key_vault_id = module.kv_shared.id
}

module "kvs_evh_brs021forwardmetereddatanotify_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "evh-brs021forwardmetereddatanotify-name"
  value        = azurerm_eventhub.evh_brs021forwardmetereddatanotify.name
  key_vault_id = module.kv_shared.id
}
