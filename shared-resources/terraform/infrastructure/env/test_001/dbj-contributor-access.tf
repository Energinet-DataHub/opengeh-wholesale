resource "azurerm_role_assignment" "DBJ-Contributor" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Contributor"
  principal_id         = "4b6a4911-0c21-4c94-a285-596cf66a4db2"
}
