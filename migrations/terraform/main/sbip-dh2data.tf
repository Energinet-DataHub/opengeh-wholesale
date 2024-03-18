resource "azurerm_storage_blob_inventory_policy" "sbip_dh2data" {
  storage_account_id = module.st_dh2data.id

  rules {
    name                   = "sbipr-dh2-timeseries"
    storage_container_name = azurerm_storage_container.dh2_timeseries.name
    format                 = "Csv"
    schedule               = "Weekly"
    scope                  = "Blob"
    filter {
      prefix_match = [azurerm_storage_container.dh2_timeseries.name]
      blob_types = ["blockBlob"]
    }
    schema_fields = [
      "Name",
      "Last-Modified",
      "Content-Length"
    ]
  }

  rules {
    name                   = "sbipr-dh2-timeseries-synchronization"
    storage_container_name = azurerm_storage_container.dh2_timeseries_synchronization.name
    format                 = "Csv"
    schedule               = "Weekly"
    scope                  = "Blob"
    filter {
      prefix_match = [azurerm_storage_container.dh2_timeseries_synchronization.name]
      blob_types = ["blockBlob"]
    }
    schema_fields = [
      "Name",
      "Last-Modified",
      "Content-Length"
    ]
  }
}
