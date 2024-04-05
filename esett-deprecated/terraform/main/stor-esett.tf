module "stor_esett" {
  source                     = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=v13"
  name                       = "data"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  ip_rules                   = local.ip_restrictions_as_string
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  account_replication_type   = "LRS"
  access_tier                = "Hot"
  account_tier               = "Standard"
  containers                 = [
    {
      name: local.blob_files_raw_container_name,
    },
    {
      name: local.blob_files_enrichments_container_name,
    },
    {
      name: local.blob_files_converted_container_name,
    },
    {
      name: local.blob_files_sent_container_name,
    },
    {
      name: local.blob_files_confirmed_container_name,
    },
    {
      name: local.blob_files_other_container_name,
    },
    {
      name: local.blob_files_error_container_name,
    },
    {
      name: local.blob_files_mga_imbalance_container_name,
    },
    {
      name: local.blob_files_brp_change_container_name,
    },
    {
      name: local.blob_files_ack_container_name
    },
  ]
}
