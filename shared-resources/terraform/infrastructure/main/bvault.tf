module "backup_vault" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/backup-vault?ref=backup-vault_6.0.1"

  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  datastore_type       = "VaultStore"
  soft_delete          = "Off"
}

module "kvs_bvault_vaulted_policy_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "bvault-vaulted-policy-id"
  value        = module.backup_vault.blob_storage_backup_vaulted_policy_id
  key_vault_id = module.kv_shared.id
}

module "kvs_bvault_vault_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "bvault-vault-id"
  value        = module.backup_vault.id
  key_vault_id = module.kv_shared.id
}

module "kvs_bvault_vault_location" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "bvault-vault-location"
  value        = azurerm_resource_group.this.location
  key_vault_id = module.kv_shared.id
}

module "kvs_bvault_vault_principal_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "bvault-vault-principal-id"
  value        = module.backup_vault.identity.0.principal_id
  key_vault_id = module.kv_shared.id
}
