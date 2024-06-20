module "storage_esett_documents" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=14.22.0"

  name                       = "documents"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  account_replication_type   = "LRS"
  access_tier                = "Hot"
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_rules                   = local.ip_restrictions_as_string
  containers = [
    {
      name = local.ESETT_DOCUMENT_STORAGE_CONTAINER_NAME
    },
    {
      name = local.ESETT_ERROR_DOCUMENT_STORAGE_CONTAINER_NAME
    },
  ]
}
