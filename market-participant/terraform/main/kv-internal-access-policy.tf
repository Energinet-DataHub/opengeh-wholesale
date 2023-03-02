module "kv_internal_access_policy_func_entrypoint_marketparticipant" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=v10"

  key_vault_id              = data.azurerm_key_vault.kv_internal.id
  app_identity              = module.func_entrypoint_marketparticipant.identity.0
}