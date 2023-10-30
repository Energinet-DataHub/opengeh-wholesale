resource "azurerm_load_test" "this" {
  location            = azurerm_resource_group.this.location
  name                = "alt-${var.domain_name_short}-${var.environment_short}-we-${var.environment_instance}"
  resource_group_name = azurerm_resource_group.this.name
}
