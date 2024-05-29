module "st_documents" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=v14"

  name                                  = "documents"
  project_name                          = var.domain_name_short
  environment_short                     = var.environment_short
  environment_instance                  = var.environment_instance
  resource_group_name                   = azurerm_resource_group.this.name
  location                              = azurerm_resource_group.this.location
  account_replication_type              = "LRS"
  private_endpoint_subnet_id            = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_rules                              = local.ip_restrictions_as_string
  prevent_deletion                      = true
  data_factory_backup = {
    id           = data.azurerm_key_vault_secret.shared_adf_id.value
    principal_id = data.azurerm_key_vault_secret.shared_adf_principal_id.value
    containers = [
      "outgoing",
      "archived"
    ]
    backup_storage_account_id           = module.st_documents_backup.id
    backup_storage_account_fqdn         = module.st_documents_backup.fully_qualified_domain_name
  }
  lifecycle_retention_delete_after_days = 3285 # 9 years = (5 + 3 + current year) * 365 days
}

module "st_documents_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=v14"

  name                                  = "backupdocs"
  project_name                          = var.domain_name_short
  environment_short                     = var.environment_short
  environment_instance                  = var.environment_instance
  resource_group_name                   = azurerm_resource_group.this.name
  location                              = azurerm_resource_group.this.location
  account_replication_type              = "LRS"
  private_endpoint_subnet_id            = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_rules                              = local.ip_restrictions_as_string
  prevent_deletion                      = true
  lifecycle_retention_delete_after_days = 3285 # 9 years = (5 + 3 + current year) * 365 days
}
