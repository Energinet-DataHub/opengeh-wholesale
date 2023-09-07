data "azurerm_subscription" "current" {
}

resource "azurerm_role_assignment" "platformteam_subscription_contributor" {
  scope                = data.azurerm_subscription.current.id
  role_definition_name = "Contributor"
  principal_id         = var.platform_team_security_group_object_id
}
