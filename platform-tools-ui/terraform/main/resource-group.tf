resource "azurerm_resource_group" "this" {
  name     = "rg-${local.name_suffix}"
  location = "West Europe"
}
