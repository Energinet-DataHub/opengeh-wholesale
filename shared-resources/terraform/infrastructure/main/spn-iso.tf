data "azuread_service_principal" "sp_datahub_iso_reader" {
  client_id = var.azure_isocontrol_spn_id
}

resource "azurerm_role_assignment" "spn_iso_reader" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Reader"
  principal_id         = data.azuread_service_principal.sp_datahub_iso_reader.object_id
}
