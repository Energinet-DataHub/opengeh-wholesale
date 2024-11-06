module "st_documents" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=storage-account-dfs_9.0.1"

  name                                  = "documents"
  project_name                          = var.domain_name_short
  environment_short                     = var.environment_short
  environment_instance                  = var.environment_instance
  resource_group_name                   = azurerm_resource_group.this.name
  location                              = azurerm_resource_group.this.location
  account_replication_type              = "LRS"
  private_endpoint_subnet_id            = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
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

module "st_documents_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=storage-account-dfs_9.0.1"

  name                                  = "backupdocs"
  project_name                          = var.domain_name_short
  environment_short                     = var.environment_short
  environment_instance                  = var.environment_instance
  resource_group_name                   = azurerm_resource_group.this.name
  location                              = azurerm_resource_group.this.location
  account_replication_type              = "LRS"
  private_endpoint_subnet_id            = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_rules                              = local.ip_restrictions_as_string
  lifecycle_retention_delete_after_days = 3285 # 9 years = (5 + 3 + current year) * 365 days
  audit_storage_account = var.enable_audit_logs ? {
    id = data.azurerm_key_vault_secret.st_audit_shres_id.value
  } : null
  prevent_deletion = false
}
