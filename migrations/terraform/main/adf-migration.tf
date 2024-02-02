resource "azurerm_data_factory" "this" {
  name                            = "adf-${local.resources_suffix}"
  location                        = azurerm_resource_group.this.location
  resource_group_name             = azurerm_resource_group.this.name
  managed_virtual_network_enabled = true
  public_network_enabled          = false
  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_data_factory_managed_private_endpoint" "adfpe_blob" {
  name               = "adfpe-blob-${local.resources_suffix}"
  data_factory_id    = azurerm_data_factory.this.id
  target_resource_id = module.st_dh2data.id
  subresource_name   = "blob"
}

resource "azurerm_role_assignment" "ra_dh2data_adf_contributor" {
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_data_factory.this.identity[0].principal_id
}
