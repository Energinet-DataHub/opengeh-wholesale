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
