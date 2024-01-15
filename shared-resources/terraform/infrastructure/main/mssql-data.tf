module "mssql_data_additional" { # Needs to be a named like this or it would delete all databases
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-server?ref=13.33.2"

  name                 = "data"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  sql_version          = "12.0"
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  monitor_action_group = {
    name                = module.ag_primary.name
    resource_group_name = azurerm_resource_group.this.name
  }

  ad_group_directory_reader = var.ad_group_directory_reader

  elastic_pool_max_size_gb      = 100
  public_network_access_enabled = true

  # If using DTU model then see pool limits based on SKU here: https://learn.microsoft.com/en-us/azure/azure-sql/database/resource-limits-dtu-elastic-pools?view=azuresql#standard-elastic-pool-limits
  elastic_pool_sku = {
    name     = "StandardPool"
    tier     = "Standard"
    capacity = 200
  }

  elastic_pool_per_database_settings = {
    min_capacity = 0
    max_capacity = 100
  }
  private_endpoint_subnet_id = data.azurerm_subnet.snet_private_endpoints.id

  depends_on = [
    module.ag_primary
  ]
}

resource "azurerm_mssql_firewall_rule" "github_largerunner" {
  count = length(split(",", local.ip_restrictions_as_string))

  name             = "github_largerunner_${count.index}"
  server_id        = module.mssql_data_additional.id
  start_ip_address = cidrhost(split(",", local.ip_restrictions_as_string)[count.index], 0)  #First IP in range
  end_ip_address   = cidrhost(split(",", local.ip_restrictions_as_string)[count.index], -1) #Last IP in range
}

module "kvs_mssql_data_elastic_pool_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "mssql-data-elastic-pool-id"
  value        = module.mssql_data_additional.elastic_pool_id
  key_vault_id = module.kv_shared.id
}

module "kvs_mssql_data_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "mssql-data-url"
  value        = module.mssql_data_additional.fully_qualified_domain_name
  key_vault_id = module.kv_shared.id
}

module "kvs_mssql_data_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "mssql-data-name"
  value        = module.mssql_data_additional.name
  key_vault_id = module.kv_shared.id
}
