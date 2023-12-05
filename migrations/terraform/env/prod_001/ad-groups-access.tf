// Peter Tandrup
resource "azurerm_role_assignment" "dh2data_storagereader" {
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = "b53ad815-1d62-4251-9ac9-962aeaa59edd"
}

// Peter Tandrup
resource "azurerm_role_assignment" "dh2dropzone_storagereader" {
  scope                = module.st_dh2dropzone.id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = "b53ad815-1d62-4251-9ac9-962aeaa59edd"
}
