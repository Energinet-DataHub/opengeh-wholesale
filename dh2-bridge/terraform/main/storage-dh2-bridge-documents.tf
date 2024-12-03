module "storage_dh2_bridge_documents" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=storage-account_7.1.1"

  name                       = "documents"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  account_replication_type   = "LRS"
  access_tier                = "Hot"
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  ip_rules                   = local.ip_restrictions_as_string
  containers = [
    {
      name = local.DH2_BRIDGE_DOCUMENT_STORAGE_CONTAINER_NAME
    },
  ]
  audit_storage_account = var.enable_audit_logs ? {
    id = data.azurerm_key_vault_secret.st_audit_shres_id.value
  } : null
  blob_storage_backup_policy = {
    backup_policy_id          = data.azurerm_key_vault_secret.bvault_vaulted_policy_id.value
    backup_vault_id           = data.azurerm_key_vault_secret.bvault_vault_id.value
    backup_vault_location     = data.azurerm_key_vault_secret.bvault_vault_location.value
    backup_vault_principal_id = data.azurerm_key_vault_secret.bvault_vault_principal_id.value
  }
}
