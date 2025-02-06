module "st_measurements" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=storage-account-dfs_9.2.0"

  name                       = "meas"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  account_replication_type   = "LRS"
  use_queue                  = true
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.kvs_snet_privateendpoints_id.value
  ip_rules                   = local.ip_restrictions_as_string
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

module "kvs_st_measurements_data_lake_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-measurements-data-lake-name"
  value        = module.st_measurements.name
  key_vault_id = module.kv_internal.id
}

#---- Containers

resource "azurerm_storage_container" "internal" {
  name                  = "internal"
  storage_account_name  = module.st_measurements.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "bronze" {
  name                  = "bronze"
  storage_account_name  = module.st_measurements.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "gold" {
  name                  = "gold"
  storage_account_name  = module.st_measurements.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "silver" {
  name                  = "silver"
  storage_account_name  = module.st_measurements.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "measurements_calculated_internal" {
  name                  = "measurements-calculated-internal"
  storage_account_name  = module.st_measurements.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "measurements_calculated" {
  name                  = "measurements-calculated"
  storage_account_name  = module.st_measurements.name
  container_access_type = "private"
}
