module "st_data_lake" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=v12"

  name                            = "datalake"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  account_replication_type        = "LRS"
  account_tier                    = "Standard"
  private_endpoint_subnet_id      = module.snet_private_endpoints.id
  private_dns_resource_group_name = module.dbw_shared.private_dns_zone_resource_group_name
  ip_rules                        = var.hosted_deployagent_public_ip_range
  role_assignments = [
    {
      principal_id         = data.azurerm_client_config.current.object_id
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
}

module "kvs_st_data_lake_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name         = "st-data-lake-name"
  value        = module.st_data_lake.name
  key_vault_id = module.kv_shared.id
}

module "kvs_st_data_lake_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name         = "st-data-lake-id"
  value        = module.st_data_lake.id
  key_vault_id = module.kv_shared.id
}
