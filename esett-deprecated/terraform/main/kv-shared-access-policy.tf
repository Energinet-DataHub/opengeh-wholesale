module "kv_shared_access_policy_func_biztalk_shipper" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=v13"

  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
  app_identity = module.func_biztalkshipper.identity.0
}
