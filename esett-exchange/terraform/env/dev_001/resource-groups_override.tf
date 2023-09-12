resource "azurerm_resource_group" "this" {
  name = "rg-main-${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  location = "West Europe"
}
