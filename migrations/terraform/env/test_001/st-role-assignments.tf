resource "azurerm_role_assignment" "st_migrations_access" {
  count                = length(var.developer_object_ids)
  principal_id         = var.developer_object_ids[count.index]
  role_definition_name = "Storage Blob Data Contributor"
  scope                = module.st_migrations.id
}

resource "azurerm_role_assignment" "st_dh2data_access" {
  count                = length(var.developer_object_ids)
  principal_id         = var.developer_object_ids[count.index]
  role_definition_name = "Storage Blob Data Contributor"
  scope                = module.st_dh2data.id
}

resource "azurerm_role_assignment" "st_dh2dropzone_access" {
  count                = length(var.developer_object_ids)
  principal_id         = var.developer_object_ids[count.index]
  role_definition_name = "Storage Blob Data Contributor"
  scope                = module.st_dh2dropzone.id
}

resource "azurerm_role_assignment" "st_dh2dropzone_archive_access" {
  count                = length(var.developer_object_ids)
  principal_id         = var.developer_object_ids[count.index]
  role_definition_name = "Storage Blob Data Contributor"
  scope                = module.st_dh2dropzone_archive.id
}
