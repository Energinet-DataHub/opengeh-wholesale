resource "azurerm_resource_group" "this" {
  name     = "rg-${local.resources_suffix}"
  location = "West Europe"

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}
