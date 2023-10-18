# Give all platformteam developers Control + Dataplane Contributor access to test_002

resource "azurerm_role_assignment" "developers_subscription_contributor" {
  scope                = data.azurerm_subscription.current.id
  role_definition_name = "Contributor"
  principal_id         = var.platform_team_security_group_object_id
}

resource "azurerm_role_assignment" "developers_blob_contributor_access" {
  scope                = data.azurerm_subscription.current.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.platform_team_security_group_object_id
}
