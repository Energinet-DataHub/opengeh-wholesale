resource "azurerm_role_assignment" "stdatalake_developer_group" {
  scope                = module.st_data_lake.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.developers_security_group_object_id
}
