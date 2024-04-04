resource "azurerm_storage_blob_inventory_policy" "sbip_dh2dropzone_archive" {
  storage_account_id = module.st_dh2dropzone_archive.id

  rules {
    name                   = "sbipr-dh2-dropzone-archive-timeseries-sync"
    storage_container_name = azurerm_storage_container.dh2_dropzone_archive_inventory_reports.name
    format                 = "Csv"
    schedule               = "Weekly"
    scope                  = "Blob"
    filter {
      prefix_match = [azurerm_storage_container.dropzonetimeseriessyncarchive.name]
      blob_types = ["blockBlob"]
    }
    schema_fields = [
      "Name",
      "Last-Modified",
      "Content-Length"
    ]
  }
}
