resource "azurerm_role_assignment" "migrations_resourcegroup_contributor" {
  scope                = azurerm_resource_group.this.id
  role_definition_name = "Contributor"
  principal_id         = var.migration_team_security_group_object_id
}

resource "azurerm_role_assignment" "migrations_storagereader" {
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = var.migration_team_security_group_object_id
}
