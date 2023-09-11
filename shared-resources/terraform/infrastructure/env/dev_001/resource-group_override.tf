resource "azurerm_resource_group" "this" {
  name     = "rg-main-${local.resources_suffix}"
  location = "West Europe"
}
