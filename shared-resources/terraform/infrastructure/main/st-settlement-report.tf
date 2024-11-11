module "st_settlement_report" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=storage-account-dfs_9.1.0"

  name                       = "settlrep"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  account_replication_type   = "GRS"
  private_endpoint_subnet_id = data.azurerm_subnet.snet_private_endpoints_002.id
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
      name = "settlement-reports"
    }
  ]

  prevent_deletion = false
}

module "kvs_st_settlement_report_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-settlement-report-name"
  value        = module.st_settlement_report.name
  key_vault_id = module.kv_shared.id
}

module "kvs_st_settlement_report_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-settlement-report-id"
  value        = module.st_settlement_report.id
  key_vault_id = module.kv_shared.id
}


module "kvs_st_settlement_report_blob_private_ip_address" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-settlement-report-blob-private-ip-address"
  value        = module.st_settlement_report.blob_private_ip_address
  key_vault_id = module.kv_shared.id
}

module "kvs_st_settlement_report_dfs_private_ip_address" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-settlement-report-dfs-private-ip-address"
  value        = module.st_settlement_report.dfs_private_ip_address
  key_vault_id = module.kv_shared.id
}
