module "mssql_data_additional" { # Needs to be a named like this or it would delete all databases
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-server?ref=mssql-server_9.0.0"

  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  sql_version          = "12.0"
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  monitor_action_group = length(module.monitor_action_group_shres) != 1 ? null : {
    id                  = module.monitor_action_group_shres[0].id
    resource_group_name = azurerm_resource_group.this.name
  }

  ad_group_directory_reader     = var.ad_group_directory_reader
  public_network_access_enabled = true

  private_endpoint_subnet_id = azurerm_subnet.privateendpoints.id
  audit_storage_account = var.enable_audit_logs ? {
    id                    = module.st_audit_logs.id
    primary_blob_endpoint = "https://${module.st_audit_logs.name}.blob.core.windows.net/"
  } : null
}

resource "azurerm_mssql_firewall_rule" "github_largerunner" {
  count = length(split(",", local.ip_restrictions_as_string))

  name             = "github_largerunner_${count.index}"
  server_id        = module.mssql_data_additional.id
  start_ip_address = cidrhost(split(",", local.ip_restrictions_as_string)[count.index], 0)  #First IP in range
  end_ip_address   = cidrhost(split(",", local.ip_restrictions_as_string)[count.index], -1) #Last IP in range
}

module "kvs_mssql_data_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "mssql-data-name"
  value        = module.mssql_data_additional.name
  key_vault_id = module.kv_shared.id
}

module "kvs_mssql_data_resource_group_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "mssql-data-resource-group-name"
  value        = azurerm_resource_group.this.name
  key_vault_id = module.kv_shared.id
}

module "kvs_mssql_data_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "mssql-data-url"
  value        = module.mssql_data_additional.fully_qualified_domain_name
  key_vault_id = module.kv_shared.id
}
