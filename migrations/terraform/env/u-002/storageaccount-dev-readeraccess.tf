
#
# Add developer read access to storage account
#
data "azuread_group" "datalake_readeraccess_group_name" {
  display_name = var.datalake_readeraccess_group_name
}

resource "azurerm_role_assignment" "datalake_readeraccess_group_name" {
  scope                = module.st_migrations.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azuread_group.datalake_readeraccess_group_name.object_id
}

resource "azurerm_role_assignment" "dh2data_readeraccess_group_name" {
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azuread_group.datalake_readeraccess_group_name.object_id
}

resource "azurerm_role_assignment" "dh2landzone_readeraccess_group_name" {
  scope                = module.st_dh2landzone.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azuread_group.datalake_readeraccess_group_name.object_id
}

resource "azurerm_role_assignment" "dh2landzonearch_readeraccess_group_name" {
  scope                = module.st_dh2landzone_archive.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azuread_group.datalake_readeraccess_group_name.object_id
}
