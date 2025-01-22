module "st_electricity_market" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=storage-account_7.1.1"

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
    { name = "electrical_heating" },
    { name = "capacity_settlement" }
  ]
}

module "kvs_st_electricity_market_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-electricity-market-url"
  value        = "https://${module.st_electricity_market.name}.blob.core.windows.net"
  key_vault_id = module.kv_shared.id
}

module "kvs_st_electricity_market_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-electricity-market-id"
  value        = module.st_electricity_market.id
  key_vault_id = module.kv_shared.id
}
