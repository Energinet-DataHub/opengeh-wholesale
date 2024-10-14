module "st_dead_letter_logs" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=storage-account_6.2.0"

  name                                  = "deadltr"
  project_name                          = var.domain_name_short
  environment_short                     = var.environment_short
  environment_instance                  = var.environment_instance
  resource_group_name                   = azurerm_resource_group.this.name
  location                              = azurerm_resource_group.this.location
  account_replication_type              = "LRS"
  private_endpoint_subnet_id            = data.azurerm_subnet.snet_private_endpoints.id
  ip_rules                              = local.ip_restrictions_as_string
  lifecycle_retention_delete_after_days = 30
  audit_storage_account = var.enable_audit_logs ? {
    id = module.st_audit_logs.id
  } : null
  role_assignments = [
    {
      principal_id         = data.azurerm_client_config.current.object_id
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
  blob_storage_backup_policy = {
    backup_policy_id          = module.backup_vault.blob_storage_backup_vaulted_policy_id
    backup_vault_id           = module.backup_vault.id
    backup_vault_location     = azurerm_resource_group.this.location
    backup_vault_principal_id = module.backup_vault.identity.0.principal_id
  }
}

module "kvs_st_deadltr_shres_blob_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_5.0.0"

  name         = "st-deadltr-shres-blob-url"
  value        = "https://${module.st_dead_letter_logs.name}.blob.core.windows.net"
  key_vault_id = module.kv_shared.id
}

module "kvs_st_deadltr_shres_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_5.0.0"

  name         = "st-deadltr-shres-id"
  value        = module.st_dead_letter_logs.id
  key_vault_id = module.kv_shared.id
}
