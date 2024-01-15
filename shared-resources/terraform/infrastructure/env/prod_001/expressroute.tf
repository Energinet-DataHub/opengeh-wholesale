resource "azurerm_express_route_circuit" "dh2_express_route" {
  name                  = "erc-${local.resources_suffix}"
  resource_group_name   = azurerm_resource_group.this.name
  location              = azurerm_resource_group.this.location
  service_provider_name = "Interxion"
  peering_location      = "Copenhagen"
  bandwidth_in_mbps     = 1000

  sku {
    tier   = "Standard"
    family = "MeteredData"
  }
}
