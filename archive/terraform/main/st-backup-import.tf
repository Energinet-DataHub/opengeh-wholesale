resource "azurerm_storage_account" "st_backup_import" {
  name                     = "stimp${local.resources_suffix_no_dash}"
  resource_group_name       = azurerm_resource_group.this.name
  location                  = azurerm_resource_group.this.location
  account_tier             = "Standard"
  account_replication_type = "GRS"
}
