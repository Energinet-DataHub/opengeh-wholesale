module "st_data_lake" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=13.32.0"

  name                            = "datalake"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  account_replication_type        = "LRS"
  account_tier                    = "Standard"
  private_endpoint_subnet_id      = data.azurerm_subnet.snet_private_endpoints.id
  ip_rules                        = local.ip_restrictions_as_string
  role_assignments = [
    {
      principal_id         = data.azurerm_client_config.current.object_id
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
}

module "kvs_st_data_lake_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "st-data-lake-name"
  value        = module.st_data_lake.name
  key_vault_id = module.kv_shared.id
}

module "kvs_st_data_lake_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "st-data-lake-id"
  value        = module.st_data_lake.id
  key_vault_id = module.kv_shared.id
}

module "kvs_private_dns_zone_resource_group_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "private-dns-zone-resource-group-name"
  value        = azurerm_resource_group.this.name
  key_vault_id = module.kv_shared.id
}

module "kvs_st_data_lake_blob_private_ip_address" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "st-data-lake-blob-private-ip-address"
  value        = module.st_data_lake.blob_private_ip_address
  key_vault_id = module.kv_shared.id
}

module "kvs_st_data_lake_dfs_private_ip_address" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "st-data-lake-dfs-private-ip-address"
  value        = module.st_data_lake.dfs_private_ip_address
  key_vault_id = module.kv_shared.id
}
