module "st_migrations_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=storage-account-dfs_9.2.0"

  name                       = "migbackup"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  account_replication_type   = "LRS"
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  ip_rules                   = local.ip_restrictions_as_string
  prevent_deletion           = false
  role_assignments = [
    {
      principal_id         = data.azurerm_key_vault_secret.shared_access_connector_principal_id.value
      role_definition_name = "Storage Blob Data Contributor"
    },
  ]
  audit_storage_account = var.enable_audit_logs ? {
    id = data.azurerm_key_vault_secret.st_audit_shres_id.value
  } : null
}

module "kvs_st_migrations_backup_data_lake_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-migrations-backup-data-lake-name"
  value        = module.st_migrations_backup.name
  key_vault_id = module.kv_internal.id
}

#---- Containers

resource "azurerm_storage_container" "internal_backup" {
  name                  = "internal-backup"
  storage_account_name  = module.st_migrations_backup.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "bronze_backup" {
  name                  = "bronze-backup"
  storage_account_name  = module.st_migrations_backup.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "silver_backup" {
  name                  = "silver-backup"
  storage_account_name  = module.st_migrations_backup.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "gold_backup" {
  name                  = "gold-backup"
  storage_account_name  = module.st_migrations_backup.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "eloverblik_backup" {
  name                  = "eloverblik-backup"
  storage_account_name  = module.st_migrations_backup.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "wholesale_backup" {
  name                  = "wholesale-backup"
  storage_account_name  = module.st_migrations_backup.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "shared_wholesale_input_backup" {
  name                  = "shared-wholesale-input-backup"
  storage_account_name  = module.st_migrations_backup.name
  container_access_type = "private"
}

#---- Role assignments

resource "azurerm_role_assignment" "ra_migrations_backup_contributor" {
  scope                = module.st_migrations_backup.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.object_id
}
