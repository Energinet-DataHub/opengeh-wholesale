module "stor_esett" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=v13-without-vnet"
  name                      = "data"
  project_name              = var.domain_name_short
  environment_short         = var.environment_short
  environment_instance      = var.environment_instance
  resource_group_name       = azurerm_resource_group.this.name
  location                  = azurerm_resource_group.this.location
  ip_rules                  = var.hosted_deployagent_public_ip_range
  account_replication_type  = "LRS"
  access_tier               = "Hot"
  account_tier              = "Standard"
  containers                = [
    local.blob_files_raw_container,
    local.blob_files_enrichments_container,
    local.blob_files_converted_container,
    local.blob_files_sent_container,
    local.blob_files_confirmed_container,
    local.blob_files_other_container,
    local.blob_files_error_container,
    local.blob_files_mga_imbalance_container,
    local.blob_files_brp_change_container,
    local.blob_files_ack_container
  ]
}

data "azurerm_storage_account_blob_container_sas" "blob_confirmed" {
  connection_string = module.stor_esett.primary_connection_string
  container_name    = local.blob_files_confirmed_container.name
  https_only        = true

  ip_address = "194.239.2.0-194.239.2.255"

  start  = "2020-01-01"
  expiry = "2099-01-01"

  permissions {
    read   = true
    add    = false
    create = false
    write  = false
    delete = false
    list   = true
  }
}

data "azurerm_storage_account_blob_container_sas" "blob_mga" {
  connection_string = module.stor_esett.primary_connection_string
  container_name    = local.blob_files_mga_imbalance_container.name
  https_only        = true

  ip_address = "194.239.2.0-194.239.2.255"

  start  = "2020-01-01"
  expiry = "2099-01-01"

  permissions {
    read   = true
    add    = false
    create = false
    write  = false
    delete = false
    list   = true
  }
}

data "azurerm_storage_account_blob_container_sas" "blob_brp" {
  connection_string = module.stor_esett.primary_connection_string
  container_name    = local.blob_files_brp_change_container.name
  https_only        = true

  ip_address = "194.239.2.0-194.239.2.255"

  start  = "2020-01-01"
  expiry = "2099-01-01"

  permissions {
    read   = true
    add    = false
    create = false
    write  = false
    delete = false
    list   = true
  }
}

data "azurerm_storage_account_blob_container_sas" "blob_sent" {
  connection_string = module.stor_esett.primary_connection_string
  container_name    = local.blob_files_sent_container.name
  https_only        = true

  ip_address = "194.239.2.0-194.239.2.255"

  start  = "2020-01-01"
  expiry = "2099-01-01"

  permissions {
    read   = true
    add    = false
    create = false
    write  = false
    delete = false
    list   = true
  }
}

data "azurerm_storage_account_blob_container_sas" "blob_other" {
  connection_string = module.stor_esett.primary_connection_string
  container_name    = local.blob_files_other_container.name
  https_only        = true

  ip_address = "194.239.2.0-194.239.2.255"

  start  = "2020-01-01"
  expiry = "2099-01-01"

  permissions {
    read   = true
    add    = false
    create = false
    write  = false
    delete = false
    list   = true
  }
}

data "azurerm_storage_account_blob_container_sas" "blob_error" {
  connection_string = module.stor_esett.primary_connection_string
  container_name    = local.blob_files_error_container.name
  https_only        = true

  ip_address = "194.239.2.0-194.239.2.255"

  start  = "2020-01-01"
  expiry = "2099-01-01"

  permissions {
    read   = true
    add    = false
    create = false
    write  = false
    delete = false
    list   = true
  }
}
