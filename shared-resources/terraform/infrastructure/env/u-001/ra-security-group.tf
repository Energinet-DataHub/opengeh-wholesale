resource "azurerm_role_assignment" "blob_reader" {
  scope                 = data.azurerm_subscription.this.id
  role_definition_name  = "Storage Blob Data Reader"
  principal_id          = var.developers_security_group_object_id
}