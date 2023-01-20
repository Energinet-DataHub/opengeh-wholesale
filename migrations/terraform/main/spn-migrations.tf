module "kvs_spn_migrations_secret" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"
  name          = "spn-migrations-secret"
  value         = var.azure_spn_secret
  key_vault_id  = module.kv_internal.id
}