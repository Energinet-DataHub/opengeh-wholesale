module "kv_internal_access_policy_app_webapi" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=v13"

  key_vault_id = module.kv_internal.id
  app_identity = module.app_wholesale_api.identity.0
}

module "kv_internal_access_policy_func_orchestration" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=v13"

  key_vault_id = module.kv_internal.id
  app_identity = module.func_wholesale_orchestration.identity.0
}
