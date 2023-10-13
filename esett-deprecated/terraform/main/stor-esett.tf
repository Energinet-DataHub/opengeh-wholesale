# NOTE: the Name used for storage needs to be globally unique, thus the random id
module "stor_esett" {
  source                   = "./modules/azure-stor-module"
  name                     = "storesett${local.name_suffix_no_dash}"
  resource_group_name      = azurerm_resource_group.this.name
  location                 = azurerm_resource_group.this.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  access_tier              = "Hot"
  containers = [
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

module "stor_esett_blob_confirmed_sas" {
  source            = "./modules/azure-stor-blob-container-sas"
  connection_string = module.stor_esett.primary_connection_string
  container_name    = local.blob_files_confirmed_container.name
  start             = "2020-01-01"
  expiry            = "2099-01-01"
  ip_address        = "194.239.2.0-194.239.2.255"
}

module "stor_esett_blob_mga_sas" {
  source            = "./modules/azure-stor-blob-container-sas"
  connection_string = module.stor_esett.primary_connection_string
  container_name    = local.blob_files_mga_imbalance_container.name
  start             = "2020-01-01"
  expiry            = "2099-01-01"
  ip_address        = "194.239.2.0-194.239.2.255"
}

module "stor_esett_blob_brp_sas" {
  source            = "./modules/azure-stor-blob-container-sas"
  connection_string = module.stor_esett.primary_connection_string
  container_name    = local.blob_files_brp_change_container.name
  start             = "2020-01-01"
  expiry            = "2099-01-01"
  ip_address        = "194.239.2.0-194.239.2.255"
}

module "stor_esett_blob_sent_sas" {
  source            = "./modules/azure-stor-blob-container-sas"
  connection_string = module.stor_esett.primary_connection_string
  container_name    = local.blob_files_sent_container.name
  start             = "2020-01-01"
  expiry            = "2099-01-01"
  ip_address        = "194.239.2.0-194.239.2.255"
}

module "stor_esett_blob_other_sas" {
  source            = "./modules/azure-stor-blob-container-sas"
  connection_string = module.stor_esett.primary_connection_string
  container_name    = local.blob_files_other_container.name
  start             = "2020-01-01"
  expiry            = "2099-01-01"
  ip_address        = "194.239.2.0-194.239.2.255"
}

module "stor_esett_blob_error_sas" {
  source            = "./modules/azure-stor-blob-container-sas"
  connection_string = module.stor_esett.primary_connection_string
  container_name    = local.blob_files_error_container.name
  start             = "2020-01-01"
  expiry            = "2099-01-01"
  ip_address        = "194.239.2.0-194.239.2.255"
}
