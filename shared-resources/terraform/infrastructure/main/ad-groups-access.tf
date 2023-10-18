# Give all developers access Controlplane Reader access to all environments

data "azurerm_subscription" "current" {
}

resource "azurerm_role_assignment" "developers_subscription_reader" {
  scope                = data.azurerm_subscription.current.id
  role_definition_name = "Reader"
  principal_id         = var.developers_security_group_object_id
}
