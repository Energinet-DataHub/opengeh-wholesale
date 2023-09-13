#
# Add developer access to storage accounts
#

resource "azurerm_role_assignment" "st_data_lake_henrik_sommer_storage_blob_data_contributor" {
  scope                = module.st_data_lake.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = "c2f75d02-f64f-4111-aaea-6c43f5bc8d65"
}
