data "azurerm_subnet" "snet_private_endpoints" {
  name                 = "snet-privateendpoint-we-001"
  virtual_network_name = var.virtual_network_name
  resource_group_name  = var.virtual_network_resource_group_name
}

data "azurerm_subnet" "snet_vnet_integration" {
  name                 = "snet-vnetintegrations-shres"
  virtual_network_name = var.virtual_network_name
  resource_group_name  = var.virtual_network_resource_group_name
}
