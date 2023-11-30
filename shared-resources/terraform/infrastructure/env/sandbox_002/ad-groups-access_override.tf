resource "azurerm_role_assignment" "developers_subscription_reader" { # This is already created due to it being an old subscription - remove this on the new sandbox
  count = 0
}
