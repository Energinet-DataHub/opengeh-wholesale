resource "azurerm_storage_blob_inventory_policy" "sbip_dh2dropzone_archive" {
  storage_account_id = module.st_dh2dropzone_archive.id

  rules {
    name                   = "sbipr-dropzonearchive"
    storage_container_name = azurerm_storage_container.dropzonearchive.name
    format                 = "Csv"
    schedule               = "Daily"
    scope                  = "Container"
    schema_fields = [
      "Name",
      "Last-Modified"
    ]
  }

  rules {
    name                   = "sbipr-dropzonetimeseriessyncarchive"
    storage_container_name = azurerm_storage_container.dropzonetimeseriessyncarchive.name
    format                 = "Csv"
    schedule               = "Daily"
    scope                  = "Container"
    schema_fields = [
      "Name",
      "Last-Modified"
    ]
  }
}
