resource "azurerm_resource_group" "this" {
  name     = "rg-${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  location = "West Europe"
}

data "azurerm_resource_group" "shared_resources" {
  name = var.shared_resources_resource_group_name
}
