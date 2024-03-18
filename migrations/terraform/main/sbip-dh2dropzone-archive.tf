resource "azurerm_storage_blob_inventory_policy" "sbip_dh2dropzone_archive" {
  storage_account_id = module.st_dh2dropzone_archive.id

  rules {
    name                   = "sbipr-dropzonearchive"
    storage_container_name = azurerm_storage_container.dropzonearchive.name
    format                 = "Csv"
    schedule               = "Weekly"
    scope                  = "Blob"
    filter {
      prefix_match = [azurerm_storage_container.dropzonearchive.name]
      blob_types = ["blockBlob"]
    }
    schema_fields = [
      "Name",
      "Last-Modified",
      "Content-Length"
    ]
  }

  rules {
    name                   = "sbipr-dropzonetimeseriessyncarchive"
    storage_container_name = azurerm_storage_container.dropzonetimeseriessyncarchive.name
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
