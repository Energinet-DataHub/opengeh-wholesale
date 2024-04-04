resource "azurerm_storage_container" "dh2_dropzone_archive_inventory_reports" {
  name                  = "inventory-reports"
  storage_account_name  = module.st_dh2dropzone_archive.name
  container_access_type = "private"
}
