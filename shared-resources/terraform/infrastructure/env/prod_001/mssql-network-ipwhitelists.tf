resource "azurerm_mssql_firewall_rule" "esett_deprecated_powerbi_rule_1" {
  name              = "Esett Deprecated PowerBI Rule #1"
  server_id         = module.mssql_data_additional.id
  start_ip_address  = "20.38.84.0"
  end_ip_address    = "20.38.84.255"
}

resource "azurerm_mssql_firewall_rule" "esett_deprecated_powerbi_rule_2" {
  name              = "Esett Deprecated PowerBI Rule #2"
  server_id         = module.mssql_data_additional.id
  start_ip_address  = "20.38.85.0"
  end_ip_address    = "20.38.85.255"
}

resource "azurerm_mssql_firewall_rule" "esett_deprecated_powerbi_rule_3" {
  name              = "Esett Deprecated PowerBI Rule #3"
  server_id         = module.mssql_data_additional.id
  start_ip_address  = "20.38.86.0"
  end_ip_address    = "20.38.86.255"
}
