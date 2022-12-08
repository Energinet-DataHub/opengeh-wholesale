data "azurerm_resource_group" "old_migrations" {
  name      = var.old_resource_group_name
}

data "azurerm_storage_account" "st_tsdropdatamig" {
  name                = "sttsdropdatmigendk${lower(var.environment_short)}"
  resource_group_name = data.azurerm_resource_group.old_migrations.name
}

resource "azurerm_role_assignment" "st_tsdropdatamig_spn" {
  scope                = data.azurerm_storage_account.st_tsdropdatamig.id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = data.azurerm_client_config.current.object_id
}