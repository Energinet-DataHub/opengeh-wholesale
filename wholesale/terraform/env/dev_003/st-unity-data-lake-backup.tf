# Backup of the wholesale data lake storage account to be used by the task force

module "st_data_backup_wholesale" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=storage-account-dfs_8.0.0"

  name                       = "databack"
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
    }
  ]

  audit_storage_account = null
  prevent_deletion      = false
}

# The storage containers are not created in the module, as they are used in schema creation. I.e., we want it dynamically
resource "azurerm_storage_container" "internal_backup" {
  name                 = "internal"
  storage_account_name = module.st_data_backup_wholesale.name
}

resource "azurerm_storage_container" "results_internal_backup" {
  name                 = "results-internal"
  storage_account_name = module.st_data_backup_wholesale.name
}

resource "azurerm_storage_container" "basis_data_internal_backup" {
  name                 = "basis-data-internal"
  storage_account_name = module.st_data_backup_wholesale.name
}

resource "azurerm_storage_container" "results_backup" {
  name                 = "results"
  storage_account_name = module.st_data_backup_wholesale.name
}

resource "azurerm_storage_container" "settlement_reports_backup" {
  name                 = "settlement-reports"
  storage_account_name = module.st_data_backup_wholesale.name
}

resource "azurerm_storage_container" "sap_backup" {
  name                 = "sap"
  storage_account_name = module.st_data_backup_wholesale.name
}

resource "azurerm_storage_container" "data_p_001_backup" {
  name                 = "data-p-001"
  storage_account_name = module.st_data_backup_wholesale.name
}

resource "azurerm_storage_container" "wholesale_migrations_wholesale_backup" {
  name                 = "wholesale-migrations-wholesale"
  storage_account_name = module.st_data_backup_wholesale.name
}
