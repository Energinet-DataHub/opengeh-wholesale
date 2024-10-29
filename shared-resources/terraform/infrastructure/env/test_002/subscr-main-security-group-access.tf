resource "azurerm_role_assignment" "platform_developers_contributor" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Contributor"
  principal_id         = data.azuread_group.platform_security_group_name.object_id
}

resource "azurerm_role_assignment" "platform_developers_locks_contributor_access" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = azurerm_role_definition.locks_contributor_access.name
  principal_id         = data.azuread_group.platform_security_group_name.object_id
}

