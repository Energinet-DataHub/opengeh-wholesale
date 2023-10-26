resource "azurerm_sql_firewall_rule" "jvm_home" {
  name                = "JVM Home"
  resource_group_name = azurerm_resource_group.this.name
  server_name         = module.mssql_data_additional.name
  start_ip_address    = "128.76.145.127"
  end_ip_address      = "128.76.145.127"
}


resource "azurerm_sql_firewall_rule" "energinet_local_ip_range" {
  name                = "Energinet local IP Range"
  resource_group_name = azurerm_resource_group.this.name
  server_name         = module.mssql_data_additional.name
  start_ip_address    = "194.239.2.0"
  end_ip_address      = "194.239.2.255"
}

resource "azurerm_sql_firewall_rule" "vesterballevej" {
  name                = "Vesterballevej"
  resource_group_name = azurerm_resource_group.this.name
  server_name         = module.mssql_data_additional.name
  start_ip_address    = "152.115.37.250"
  end_ip_address      = "152.115.37.250"
}

resource "azurerm_sql_firewall_rule" "systemate" {
  name                = "Systemate"
  resource_group_name = azurerm_resource_group.this.name
  server_name         = module.mssql_data_additional.name
  start_ip_address    = "93.160.57.54"
  end_ip_address      = "93.160.57.54"
}

resource "azurerm_sql_firewall_rule" "esett_deprecated_powerbi_rule_1" {
  name                = "Esett Deprecated PowerBI Rule #1"
  resource_group_name = azurerm_resource_group.this.name
  server_name         = module.mssql_data_additional.name
  start_ip_address    = "20.38.84.0"
  end_ip_address      = "20.38.84.255"
}

resource "azurerm_sql_firewall_rule" "esett_deprecated_powerbi_rule_2" {
  name                = "Esett Deprecated PowerBI Rule #2"
  resource_group_name = azurerm_resource_group.this.name
  server_name         = module.mssql_data_additional.name
  start_ip_address    = "20.38.85.0"
  end_ip_address      = "20.38.85.255"
}

resource "azurerm_sql_firewall_rule" "esett_deprecated_powerbi_rule_3" {
  name                = "Esett Deprecated PowerBI Rule #3"
  resource_group_name = azurerm_resource_group.this.name
  server_name         = module.mssql_data_additional.name
  start_ip_address    = "20.38.86.0"
  end_ip_address      = "20.38.86.255"
}
