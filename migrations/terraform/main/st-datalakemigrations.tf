data "azurerm_storage_account" "datalakemigrations" {
  name                = "stdatalakemigrations${lower(var.environment_short)}${lower(var.environment_instance)}"
  resource_group_name = azurerm_resource_group.this.name
}

resource "azurerm_role_assignment" "st_datalakemigrations_spn" {
  scope                = data.azurerm_storage_account.datalakemigrations.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azurerm_client_config.current.object_id
}

resource "azurerm_role_assignment" "st_datalakemigrations_securitygroup" {
  scope                 = data.azurerm_storage_account.datalakemigrations.id
  role_definition_name  = "Storage Blob Data Contributor"
  principal_id          = var.azure_ad_security_group_id
}