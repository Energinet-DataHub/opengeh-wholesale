#
# Add developer access to storage account
#
resource "azurerm_role_assignment" "dh2data_henrik_sommer_storage_blob_data_contributor" {
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = "c2f75d02-f64f-4111-aaea-6c43f5bc8d65"
}

resource "azurerm_role_assignment" "dh2data_dan_stenroejl_storage_blob_data_contributor" {
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = "7f417f9a-36c2-40c4-bd81-3d2fc95cc113"
}
