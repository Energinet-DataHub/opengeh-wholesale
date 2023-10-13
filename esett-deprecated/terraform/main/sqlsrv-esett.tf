locals {
  sqlServerAdminName = "esettdbadmin"
}

module "sqlsrv_esett" {
  source                        = "./modules/sql-server"
  name                          = "sqlsrv-esett-${local.name_suffix}"
  resource_group_name           = azurerm_resource_group.this.name
  location                      = azurerm_resource_group.this.location
  administrator_login           = local.sqlServerAdminName
  administrator_login_password  = random_password.sqlsrv_admin_password.result
}

module "sqldb_esett" {
  source              = "./modules/sql-database"
  name                = "sqldb-esett-${local.name_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  server_name         = module.sqlsrv_esett.name
  server_id           = module.sqlsrv_esett.id
  dependencies        = [module.sqlsrv_esett.dependent_on]
}

module "sqlsrv_admin_username" {
  source        = "./modules/key-vault-secret"
  name          = "SQLSERVER--ADMIN--USER"
  value         = local.sqlServerAdminName
  key_vault_id  = module.kv_esett.id
}

module "sqlsrv_admin_password" {
  source        = "./modules/key-vault-secret"
  name          = "SQLSERVER--ADMIN--PASSWORD"
  value         = random_password.sqlsrv_admin_password.result
  key_vault_id  = module.kv_esett.id
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
  server_name         = module.sqlsrv_esett.name
  start_ip_address    = "0.0.0.0"
  end_ip_address      = "0.0.0.0"
}

resource "azurerm_sql_firewall_rule" "sqlsrv-firewall-rule-external" {
  name                = "Energinet local IP range"
  resource_group_name = azurerm_resource_group.this.name
  server_name         = module.sqlsrv_esett.name
  start_ip_address    = "194.239.2.0"
  end_ip_address      = "194.239.2.255"
}
