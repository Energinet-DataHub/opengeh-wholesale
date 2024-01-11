resource "azurerm_resource_group" "this" {
  name     = "rg-${local.resources_suffix}"
  location = "West Europe"
}

resource "azurerm_role_assignment" "xjdml" {
  scope                = azurerm_resource_group.this.id
  role_definition_name = "Contributor"
  principal_id         = "e1834474-72e9-4edd-b49a-d8b959e2b6c3"
}
