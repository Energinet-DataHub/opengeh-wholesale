resource "azurerm_resource_group" "this" {
  name      = var.resource_group_name
  location  = "West Europe"
}

data "azurerm_resource_group" "shared_resources" {
  name = var.shared_resources_resource_group_name
}
