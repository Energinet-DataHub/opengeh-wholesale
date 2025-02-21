module "st_data_lake" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=storage-account-dfs_10.0.0"

  name                       = "datalake"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  account_replication_type   = "LRS"
  prevent_deletion           = false
  private_endpoint_subnet_id = azurerm_subnet.privateendpoints.id
  ip_rules                   = local.ip_restrictions_as_string
  audit_storage_account = var.enable_audit_logs ? {
    id = module.st_audit_logs.id
  } : null
  role_assignments = [
    {
      principal_id         = data.azurerm_client_config.current.object_id
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
  containers = [
    {
      name = "unityroot"
    },
    {
      name = "wholesaleinput"
    }
  ]
}

module "kvs_st_data_lake_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-data-lake-name"
  value        = module.st_data_lake.name
  key_vault_id = module.kv_shared.id
}

module "kvs_st_data_lake_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-data-lake-id"
  value        = module.st_data_lake.id
  key_vault_id = module.kv_shared.id
}
