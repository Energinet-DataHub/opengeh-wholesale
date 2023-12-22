resource "azurerm_public_ip_prefix" "vnet_integration_public_ip_prefix" {
  name                = "ippre-vnetintegration-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  prefix_length       = 28
}

resource "azurerm_nat_gateway" "nat_gateway" {
  name                = "ng-vnetintegration-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  sku_name            = "Standard"
}

resource "azurerm_nat_gateway_public_ip_prefix_association" "nat_gateway_public_ip_prefix_association" {
  public_ip_prefix_id = azurerm_public_ip_prefix.vnet_integration_public_ip_prefix.id
  nat_gateway_id      = azurerm_nat_gateway.nat_gateway.id
}

resource "azurerm_subnet_nat_gateway_association" "vnet_integration_nat_gateway_association" {
  subnet_id      = data.azurerm_subnet.snet_vnet_integration.id
  nat_gateway_id = azurerm_nat_gateway.nat_gateway.id
}
