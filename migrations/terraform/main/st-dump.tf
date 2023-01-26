data "azurerm_resource_group" "migrations" {
  name      = "rg-DataHub-DataMigration-${upper(var.environment_short)}"
}

data "azurerm_storage_account" "drop" {
  name                = "sttsdropdatmigendk${lower(var.environment_short)}"
  resource_group_name = data.azurerm_resource_group.migrations.name
}

resource "azurerm_role_assignment" "ra_drop_reader" {
  scope                = data.azurerm_storage_account.drop.id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = azuread_service_principal.spn_databricks.id
}