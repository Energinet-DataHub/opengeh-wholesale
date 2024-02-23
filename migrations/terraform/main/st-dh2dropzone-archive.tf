module "st_dh2dropzone_archive" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=v13"

  name                            = "dh2dropznarch"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  account_replication_type        = "LRS"
  account_tier                    = "Standard"
  access_tier                     = "Cool"
  private_endpoint_subnet_id      = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_rules                        = local.ip_restrictions_as_string
  prevent_deletion                = true
}

#---- Role assignments

resource "azurerm_role_assignment" "ra_dh2dropzonearch_contributor" {
  scope                = module.st_dh2dropzone_archive.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.id
}

#---- Containers

resource "azurerm_storage_container" "dropzonearchive" {
  name                  = "dropzonearchive"
  storage_account_name  = module.st_dh2dropzone_archive.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "dropzonetimeseriessyncarchive" {
  name                  = "dropzonetimeseriessyncarchive"
  storage_account_name  = module.st_dh2dropzone_archive.name
  container_access_type = "private"
}
