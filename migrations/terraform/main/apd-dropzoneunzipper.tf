resource "azurerm_portal_dashboard" "dropzoneunzipper" {
  name                = "apd-dropzoneunzipper-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  dashboard_properties = templatefile("dashboard-templates/dropzoneunzipper_dashboard.tpl",
    {
      dropzoneunzipper_id                = module.func_dropzoneunzipper.id,
      dropzoneunzipper_name              = module.func_dropzoneunzipper.name,
      evhns_dropzone_id                  = azurerm_eventhub_namespace.eventhub_namespace_dropzone.id,
      evhns_dropzone_name                = azurerm_eventhub_namespace.eventhub_namespace_dropzone.name,
      evh_dropzone_name                  = azurerm_eventhub.eventhub_dropzone_zipped.name,
      appi_sharedres_id                  = data.azurerm_key_vault_secret.appi_id.value,
  })
}
