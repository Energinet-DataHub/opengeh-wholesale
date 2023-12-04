# Give all developers Storage Blob Data Reader access to dev_001

resource "azurerm_role_assignment" "developers_blob_read_access" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = var.developers_security_group_object_id
}
