module "st_documents" {
  data_factory_backup = {
    enabled      = true
    id           = data.azurerm_key_vault_secret.shared_adf_id.value
    principal_id = data.azurerm_key_vault_secret.shared_adf_principal_id.value
    containers = [
      "outgoing",
      "archived"
    ]
    backup_storage_account_id   = module.st_documents_backup.id
    backup_storage_account_fqdn = module.st_documents_backup.fully_qualified_domain_name
  }
}
