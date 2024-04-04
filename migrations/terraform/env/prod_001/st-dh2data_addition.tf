resource "azurerm_storage_container" "dh2_inventory_reports" {
  name                  = "inventory-reports"
  storage_account_name  = module.st_dh2data.name
  container_access_type = "private"
}
