module "st_dbw_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=storage-account-dfs_4.0.1"

  name                       = "dbwbackup"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  account_replication_type   = "GRS"
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_rules                   = local.ip_restrictions_as_string

  role_assignments = [
    {
      principal_id         = data.azurerm_key_vault_secret.shared_access_connector_principal_id.value
      role_definition_name = "Storage Blob Data Contributor"
    },
  ]
}

resource "azurerm_storage_container" "backup_results_internal" {
  name                  = azurerm_storage_container.results_internal.name
  storage_account_name  = module.st_dbw_backup.name
  container_access_type = "private"
}
