
data "azuread_user" "xrss_aadadmin" {
  user_principal_name = "XRSS-aadadmin@energinet.dk"
}

resource "azurerm_role_assignment" "xrss_aadadmin_st_sapbi_blob_reader" {
  scope                = module.st_sapbi.id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = data.azuread_user.xrss_aadadmin.object_id
}

resource "azurerm_role_assignment" "xrss_aadadmin_st_sapbi_blob_contributor" {
  scope                = module.st_sapbi.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azuread_user.xrss_aadadmin.object_id
}
