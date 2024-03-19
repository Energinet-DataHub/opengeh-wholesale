module "backup_vault" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/backup-vault?ref=13.55.0"

  project_name          = var.domain_name_short
  environment_short     = var.environment_short
  environment_instance  = var.environment_instance
  resource_group_name   = azurerm_resource_group.this.name
  location              = azurerm_resource_group.this.location
  datastore_type        = "VaultStore"
}

module "kvs_bvault_policy_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.55.0"

  name         = "bvault-policy-id"
  value        = module.backup_vault.blob_storage_backup_policy_ids["DataHubDefault"]
  key_vault_id = module.kv_shared.id
}

module "kvs_bvault_vault_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.55.0"

  name         = "bvault-vault-id"
  value        = module.backup_vault.id
  key_vault_id = module.kv_shared.id
}

module "kvs_bvault_vault_location" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.55.0"

  name         = "bvault-vault-location"
  value        = azurerm_resource_group.this.location
  key_vault_id = module.kv_shared.id
}

module "kvs_bvault_vault_principal_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.55.0"

  name         = "bvault-vault-principal-id"
  value        = module.backup_vault.identity.0.principal_id
  key_vault_id = module.kv_shared.id
}
