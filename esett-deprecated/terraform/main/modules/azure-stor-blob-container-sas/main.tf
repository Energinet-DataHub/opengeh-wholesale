data "azurerm_storage_account_blob_container_sas" "main" {
  connection_string = var.connection_string
  container_name    = var.container_name
  https_only        = true

  ip_address = var.ip_address

  start  = var.start
  expiry = var.expiry

  permissions {
    read   = true
    add    = false
    create = false
    write  = false
    delete = false
    list   = true
  }
}
