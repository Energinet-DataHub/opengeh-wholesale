module "kv_shared_access_policy_func_entrypoint_peek" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=v13"

  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
  app_identity = module.func_entrypoint_peek.identity.0
}

module "kv_shared_access_policy_func_entrypoint_exchange_event_receiver" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=v13"

  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
  app_identity = module.func_entrypoint_exchange_event_receiver.identity.0
}

module "kv_shared_access_policy_func_entrypoint_ecp_inbox" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=v13"

  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
  app_identity = module.func_entrypoint_ecp_inbox.identity.0
}

module "kv_shared_access_policy_func_entrypoint_ecp_outbox" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=v13"

  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
  app_identity = module.func_entrypoint_ecp_outbox.identity.0
}

module "kv_shared_access_policy_app_webapi" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=v13"

  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
  app_identity = module.app_webapi.identity.0
}
