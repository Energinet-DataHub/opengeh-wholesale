module "st_data_lake" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=v11"

  name                            = "datalake"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  account_replication_type        = "LRS"
  account_tier                    = "Standard"
  is_hns_enabled                  = true
  log_analytics_workspace_id      = module.log_workspace_shared.id
  private_endpoint_subnet_id      = module.snet_private_endpoints.id
  private_dns_resource_group_name = module.dbw_shared.private_dns_zone_resource_group_name
  ip_rules                        = var.hosted_deployagent_public_ip_range
}

module "kvs_st_data_lake_primary_connection_string" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v11"

  name         = "st-data-lake-primary-connection-string"
  value        = module.st_data_lake.primary_connection_string
  key_vault_id = module.kv_shared.id
}

module "kvs_st_data_lake_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v11"

  name         = "st-data-lake-name"
  value        = module.st_data_lake.name
  key_vault_id = module.kv_shared.id
}

module "kvs_st_data_lake_primary_access_key" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v11"

  name         = "st-data-lake-primary-access-key"
  value        = module.st_data_lake.primary_access_key
  key_vault_id = module.kv_shared.id
}

module "kvs_st_data_lake_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v11"

  name         = "st-data-lake-id"
  value        = module.st_data_lake.id
  key_vault_id = module.kv_shared.id
}

resource "azurerm_role_assignment" "st_datalake_spn" {
  scope                = module.st_data_lake.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azurerm_client_config.current.object_id
}
