# Old, HNS-enabled storage account
module "st_documents" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=storage-account-dfs_10.0.0"

  name                                  = "documents"
  project_name                          = var.domain_name_short
  environment_short                     = var.environment_short
  environment_instance                  = var.environment_instance
  resource_group_name                   = azurerm_resource_group.this.name
  location                              = azurerm_resource_group.this.location
  account_replication_type              = "LRS"
  private_endpoint_subnet_id            = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  ip_rules                              = local.ip_restrictions_as_string
  lifecycle_retention_delete_after_days = 3285 # 9 years = (5 + 3 + current year) * 365 days
  audit_storage_account = var.enable_audit_logs ? {
    id = data.azurerm_key_vault_secret.st_audit_shres_id.value
  } : null

  prevent_deletion = false
}

resource "azurerm_storage_container" "outgoing" {
  name                  = "outgoing"
  storage_account_name  = module.st_documents.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "archived" {
  name                  = "archived"
  storage_account_name  = module.st_documents.name
  container_access_type = "private"
}

# Old, HNS-enabled storage account for backup
module "st_documents_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=storage-account-dfs_10.0.0"

  name                                  = "backupdocs"
  project_name                          = var.domain_name_short
  environment_short                     = var.environment_short
  environment_instance                  = var.environment_instance
  resource_group_name                   = azurerm_resource_group.this.name
  location                              = azurerm_resource_group.this.location
  account_replication_type              = "LRS"
  private_endpoint_subnet_id            = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  ip_rules                              = local.ip_restrictions_as_string
  lifecycle_retention_delete_after_days = 3285 # 9 years = (5 + 3 + current year) * 365 days
  audit_storage_account = var.enable_audit_logs ? {
    id = data.azurerm_key_vault_secret.st_audit_shres_id.value
  } : null

  prevent_deletion = false
}

# New standard storage account with backup enabled
module "st_docs" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=storage-account_8.0.0"

  name                 = "docs"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  # https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-storage-providers
  account_replication_type              = "GRS"
  private_endpoint_subnet_id            = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  ip_rules                              = local.ip_restrictions_as_string
  lifecycle_retention_delete_after_days = 3285 # 9 years = (5 + 3 + current year) * 365 days
  containers = [
    {
      name = "outgoing"
    },
    {
      name = "archived"
    }
  ]
  blob_storage_backup_policy = {
    backup_policy_id          = data.azurerm_key_vault_secret.bvault_vaulted_policy_id.value
    backup_vault_id           = data.azurerm_key_vault_secret.bvault_vault_id.value
    backup_vault_location     = data.azurerm_key_vault_secret.bvault_vault_location.value
    backup_vault_principal_id = data.azurerm_key_vault_secret.bvault_vault_principal_id.value
  }
  audit_storage_account = var.enable_audit_logs ? {
    id = data.azurerm_key_vault_secret.st_audit_shres_id.value
  } : null

  prevent_deletion = false
}
