#
# Add developer access to storage accounts
#

# st_dh2data
resource "azurerm_role_assignment" "st_dh2dropzone_henrik_sommer_storage_blob_data_contributor" {
  scope                = module.st_dh2dropzone.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = "c2f75d02-f64f-4111-aaea-6c43f5bc8d65"
}

resource "azurerm_role_assignment" "st_dh2dropzone_peter_tandrup_storage_blob_data_contributor" {
  scope                = module.st_dh2dropzone.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = "b53ad815-1d62-4251-9ac9-962aeaa59edd"
}

# Henrik Sommer (XHESO)
resource "azurerm_role_assignment" "st_migrations_henrik_sommer_storage_blob_data_contributor" {
  scope                = module.st_migrations.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = "c2f75d02-f64f-4111-aaea-6c43f5bc8d65"
}

# Peter Tandrup (PTA)
resource "azurerm_role_assignment" "st_migrations_peter_tandrup_storage_blob_data_contributor" {
  scope                = module.st_migrations.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = "b53ad815-1d62-4251-9ac9-962aeaa59edd"
}
