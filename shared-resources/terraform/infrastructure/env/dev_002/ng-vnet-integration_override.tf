resource "azurerm_public_ip_prefix" "vnet_integration_public_ip_prefix" {
  name                = "ippre-vnetintegration-${local.resources_suffix}"
}

resource "azurerm_nat_gateway" "nat_gateway" {
  name                = "ng-vnetintegration-${local.resources_suffix}"
}

resource "azurerm_subnet_nat_gateway_association" "vnet_integration_nat_gateway_association" {
  subnet_id      = data.azurerm_subnet.snet_vnet_integration.id
}
