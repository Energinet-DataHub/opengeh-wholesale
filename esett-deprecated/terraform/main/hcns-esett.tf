resource "azurerm_relay_namespace" "relay" {
  name                = "hc-relaynamespace-${local.name_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  sku_name            = "Standard"

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}
