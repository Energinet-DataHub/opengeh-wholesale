locals {
  sqlServerAdminName = "esettdbadmin"
}

module "mssql_esett" {
  source                = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-server?ref=v13-without-vnet"

  project_name          = var.domain_name_short
  environment_short     = var.environment_short
  environment_instance  = var.environment_instance
  sql_version           = "12.0"
  resource_group_name   = azurerm_resource_group.this.name
  location              = azurerm_resource_group.this.location
  monitor_action_group  = {
    name                = data.azurerm_key_vault_secret.ag_primary_name.value
    resource_group_name = var.shared_resources_resource_group_name
  }

  administrator_login          = local.sqlServerAdminName
  administrator_login_password = random_password.sqlsrv_admin_password.result

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
    max_capacity = 100
  }
}

module "kvs_sqlsrv_admin_username" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name          = "SQLSERVER--ADMIN--USER"
  value         = local.sqlServerAdminName
  key_vault_id  = module.kv_esett.id
}

module "kvs_sqlsrv_admin_password" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name          = "SQLSERVER--ADMIN--PASSWORD"
  value         = random_password.sqlsrv_admin_password.result
  key_vault_id  = module.kv_esett.id
}

module "mssqldb_esett" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=v12"

  name                               = "data"
  location                           = azurerm_resource_group.this.location
  project_name                       = var.domain_name_short
  environment_short                  = var.environment_short
  environment_instance               = var.environment_instance
  server_id                          = module.mssql_esett.id
  sql_server_name                    = module.mssql_esett.name
  elastic_pool_id                    = module.mssql_esett.elastic_pool_id
  monitor_alerts_action_group_id     = data.azurerm_key_vault_secret.ag_primary_id.value
  monitor_alerts_resource_group_name = azurerm_resource_group.this.name
}

resource "random_password" "sqlsrv_admin_password" {
  length = 16
  special = true
  override_special = "$"
  min_special = 2
  min_numeric = 2
  min_upper = 2
  min_lower = 2
}

resource "azurerm_sql_firewall_rule" "sqlsrv-firewall-rule-internal" {
  name                = "Azure-internal IP adresses"
  resource_group_name = azurerm_resource_group.this.name
  server_name         = module.mssql_esett.name
  start_ip_address    = "0.0.0.0"
  end_ip_address      = "0.0.0.0"
}

resource "azurerm_sql_firewall_rule" "sqlsrv-firewall-rule-external" {
  name                = "Energinet local IP range"
  resource_group_name = azurerm_resource_group.this.name
  server_name         = module.mssql_esett.name
  start_ip_address    = "194.239.2.0"
  end_ip_address      = "194.239.2.255"
}
