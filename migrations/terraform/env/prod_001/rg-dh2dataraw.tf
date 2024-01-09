# Resourcegroup for Migration to contain legacy staggredropdatmigendkp account
resource "azurerm_resource_group" "rg_dh2dataraw" {
  name     = "rg-dh2dataraw-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  location = "West Europe"
}
