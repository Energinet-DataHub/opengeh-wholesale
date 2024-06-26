resource "azurerm_role_assignment" "platform_developers_contributor" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Contributor"
  principal_id         = data.azuread_group.platform_security_group_name.object_id
}
