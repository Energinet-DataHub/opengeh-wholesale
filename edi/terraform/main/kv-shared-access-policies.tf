module "kv_shared_access_policy_func_receiver" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=v12"

  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
  app_identity = module.func_receiver.identity.0
}

module "kv_shared_acess_policy_b2c_web_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=v12"

  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
  app_identity = module.b2c_web_api.identity.0
}
