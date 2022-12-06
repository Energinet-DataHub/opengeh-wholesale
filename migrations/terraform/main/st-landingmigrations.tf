data "azurerm_storage_account" "landingmigrations" {
  name                = "stlandingmigrations${lower(var.environment_short)}${lower(var.environment_instance)}"
  resource_group_name = azurerm_resource_group.this.name
}

resource "azurerm_role_assignment" "st_landingmigrations_spn" {
  scope                = data.azurerm_storage_account.landingmigrations.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azurerm_client_config.current.object_id
}