module "st_data_wholesale" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=storage-account-dfs_9.2.0"

  name                       = "data"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  account_replication_type   = "GRS"
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  ip_rules                   = local.ip_restrictions_as_string
  role_assignments = [
    {
      principal_id         = data.azurerm_key_vault_secret.shared_access_connector_principal_id.value
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
  audit_storage_account = var.enable_audit_logs ? {
    id = data.azurerm_key_vault_secret.st_audit_shres_id.value
  } : null
  prevent_deletion = false
}

# The storage containers are not created in the module, as they are used in schema creation. I.e., we want it dynamically
resource "azurerm_storage_container" "internal" {
  name                 = "internal"
  storage_account_name = module.st_data_wholesale.name
}

resource "azurerm_storage_container" "results_internal" {
  name                 = "results-internal"
  storage_account_name = module.st_data_wholesale.name
}

resource "azurerm_storage_container" "basis_data_internal" {
  name                 = "basis-data-internal"
  storage_account_name = module.st_data_wholesale.name
}

resource "azurerm_storage_container" "basis_data" {
  name                 = "basis-data"
  storage_account_name = module.st_data_wholesale.name
}

resource "azurerm_storage_container" "results" {
  name                 = "results"
  storage_account_name = module.st_data_wholesale.name
}

resource "azurerm_storage_container" "settlement_reports" {
  name                 = "settlement-reports"
  storage_account_name = module.st_data_wholesale.name
}

resource "azurerm_storage_container" "sap" {
  name                 = "sap"
  storage_account_name = module.st_data_wholesale.name
}

data "azurerm_key_vault_secret" "shared_access_connector_principal_id" {
  name         = "shared-access-connector-principal-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
