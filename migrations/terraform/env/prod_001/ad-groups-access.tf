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

# Allows XKBER to move staggredropdatmigendkp from P-001 to prod_001
resource "azurerm_role_assignment" "xkber_contributor_rgdh2dataraw" {
  scope                = azurerm_resource_group.rg_dh2dataraw.id
  role_definition_name = "Contributor"
  principal_id         = "787bf5d7-ea80-45dc-b6da-c8e1bc60efdd"
}
