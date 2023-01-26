module "kv_shared_access_policy_app_webapi" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=v10"

  key_vault_id              = data.azurerm_key_vault.kv_shared_resources.id
  app_identity              = module.app_wholesale_api.identity.0
}

module "kv_shared_access_policy_func_processmanager" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=v10"

  key_vault_id              = data.azurerm_key_vault.kv_shared_resources.id
  app_identity              = module.func_processmanager.identity.0
}
