module "mssql_data" {
  count = 0

  private_endpoint_subnet_id = data.azurerm_subnet.snet_private_endpoints.id # Needed to override old variable as the subnet module no longer exists
}

resource "azurerm_mssql_firewall_rule" "github_largerunner" {
  server_id = module.mssql_data_additional.id
}

module "kvs_mssql_data_url" {
  value = module.mssql_data_additional.fully_qualified_domain_name
}

module "kvs_mssql_data_name" {
  value = module.mssql_data_additional.name
}

module "kvs_mssql_data_elastic_pool_id" {
  value = module.mssql_data_additional.elastic_pool_id
}
