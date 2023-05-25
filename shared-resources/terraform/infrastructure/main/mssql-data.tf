locals {
  mssqlServerAdminName = "gehdbadmin"
}

module "mssql_data" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-server?ref=v11"

  name                           = "data"
  project_name                   = var.domain_name_short
  environment_short              = var.environment_short
  environment_instance           = var.environment_instance
  sql_version                    = "12.0"
  resource_group_name            = azurerm_resource_group.this.name
  location                       = azurerm_resource_group.this.location
  monitor_alerts_action_group_id = module.ag_primary.id

  administrator_login          = local.mssqlServerAdminName
  administrator_login_password = random_password.mssql_administrator_login_password.result

  ad_authentication_only    = false
  ad_group_directory_reader = var.ad_group_directory_reader

  private_endpoint_subnet_id = module.snet_private_endpoints.id
  log_analytics_workspace_id = module.log_workspace_shared.id

  elastic_pool_max_size_gb      = 100
  public_network_access_enabled = true


  # If using DTU model then see pool limits based on SKU here: https://learn.microsoft.com/en-us/azure/azure-sql/database/resource-limits-dtu-elastic-pools?view=azuresql#standard-elastic-pool-limits
  elastic_pool_sku = {
    name     = "StandardPool"
    tier     = "Standard"
    capacity = 100
  }

  elastic_pool_per_database_settings = {
    min_capacity = 0
    max_capacity = 50
  }
}

resource "azurerm_mssql_firewall_rule" "github_largerunner" {
  count = length(split(",", var.hosted_deployagent_public_ip_range))

  name             = "github_largerunner_${count.index}"
  server_id        = module.mssql_data.id
  start_ip_address = cidrhost(split(",", var.hosted_deployagent_public_ip_range)[count.index], 0)  #First IP in range
  end_ip_address   = cidrhost(split(",", var.hosted_deployagent_public_ip_range)[count.index], -1) #Last IP in range
}

resource "random_password" "mssql_administrator_login_password" {
  length           = 16
  special          = true
  override_special = "_%@"
}

module "kvs_mssql_data_elastic_pool_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v11"

  name         = "mssql-data-elastic-pool-id"
  value        = module.mssql_data.elastic_pool_id
  key_vault_id = module.kv_shared.id
}

module "kvs_mssql_data_admin_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v11"

  name         = "mssql-data-admin-user-name"
  value        = local.mssqlServerAdminName
  key_vault_id = module.kv_shared.id
}

module "kvs_mssql_data_admin_password" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v11"

  name         = "mssql-data-admin-user-password"
  value        = random_password.mssql_administrator_login_password.result
  key_vault_id = module.kv_shared.id
}

module "kvs_mssql_data_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v11"

  name         = "mssql-data-url"
  value        = module.mssql_data.fully_qualified_domain_name
  key_vault_id = module.kv_shared.id
}

module "kvs_mssql_data_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v11"

  name         = "mssql-data-name"
  value        = module.mssql_data.name
  key_vault_id = module.kv_shared.id
}
