#
# Add developer read access to storage account on t-001
#
locals {
  deploy_readeraccess_count = var.datalake_readeraccess_group_name == "" ? 0 : 1
}

data "azuread_group" "datalake_readeraccess_group_name" {
  count        = local.deploy_readeraccess_count
  display_name = var.datalake_readeraccess_group_name == "" ? null : var.datalake_readeraccess_group_name
}

resource "azurerm_role_assignment" "datalake_readeraccess_group_name" {
  count                = local.deploy_readeraccess_count
  scope                = module.st_migrations.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azuread_group.datalake_readeraccess_group_name[count.index].object_id
}

resource "azurerm_role_assignment" "dh2data_readeraccess_group_name" {
  count                = local.deploy_readeraccess_count
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azuread_group.datalake_readeraccess_group_name[count.index].object_id
}

resource "azurerm_role_assignment" "dh2dropzone_readeraccess_group_name" {
  count                = local.deploy_readeraccess_count
  scope                = module.st_dh2dropzone.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azuread_group.datalake_readeraccess_group_name[count.index].object_id
}

resource "azurerm_role_assignment" "dh2dropzonearch_readeraccess_group_name" {
  count                = local.deploy_readeraccess_count
  scope                = module.st_dh2dropzone_archive.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azuread_group.datalake_readeraccess_group_name[count.index].object_id
}
