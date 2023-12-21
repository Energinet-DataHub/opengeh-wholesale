resource "azurerm_resource_group" "this" {
  name     = "rg-${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  location = "West Europe"
}

data "azurerm_resource_group" "shared" {
  name = "rg-shres-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
}
