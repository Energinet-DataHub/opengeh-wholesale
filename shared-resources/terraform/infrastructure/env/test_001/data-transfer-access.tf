# Give developers dataplane access to test_001

# Xrasf privileges
resource "azurerm_role_assignment" "xrasf_data_contributor" {
  scope                = module.st_data_lake.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = "3b552836-c603-40f0-b936-86deaa13663b"
}
