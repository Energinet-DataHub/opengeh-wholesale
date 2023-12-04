# Give all developers Contributor Control + Dataplane access to sand_002

resource "azurerm_role_assignment" "developers_subscription_contributor" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Contributor"
  principal_id         = var.developers_security_group_object_id
}

resource "azurerm_role_assignment" "developers_blob_contributor_access" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.developers_security_group_object_id
}
