resource "azurerm_resource_group" "this" {
  name = "rg-${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  location = "West Europe"
}

resource "azurerm_role_assignment" "developer_access" {
  for_each = toset(var.developer_object_ids)

  scope                = azurerm_resource_group.this.id
  role_definition_name = "Contributor"
  principal_id         = each.value
}
