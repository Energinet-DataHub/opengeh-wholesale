module "st_electricity_market" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=storage-account-dfs_9.2.0"

  name                       = "elmarket"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  account_replication_type   = "ZRS"
  access_tier                = "Hot"
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
    { name = local.st_electricity_market_electrical_heating_container_name },
    { name = local.st_electricity_market_capacity_settlement_container_name }
  ]
}

locals {
  st_electricity_market_electrical_heating_container_name  = "electrical-heating"
  st_electricity_market_capacity_settlement_container_name = "capacity-settlement"
}

module "kvs_st_electricity_market_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-electricity-market-url"
  value        = "https://${module.st_electricity_market.name}.blob.core.windows.net"
  key_vault_id = module.kv_shared.id
}

module "kvs_st_electricity_market_electrical_heating_container_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-electricity-market-electrical-heating-container-name"
  value        = local.st_electricity_market_electrical_heating_container_name
  key_vault_id = module.kv_shared.id
}

module "kvs_st_electricity_market_capacity_settlement_container_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-electricity-market-capacity-settlement-container-name"
  value        = local.st_electricity_market_capacity_settlement_container_name
  key_vault_id = module.kv_shared.id
}

module "kvs_st_electricity_market_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-electricity-market-name"
  value        = module.st_electricity_market.name
  key_vault_id = module.kv_shared.id
}

module "kvs_st_electricity_market_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-electricity-market-id"
  value        = module.st_electricity_market.id
  key_vault_id = module.kv_shared.id
}
