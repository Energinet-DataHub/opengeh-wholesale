module "st_key_vault_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=13.60.1"

  name                       = "kvbackup"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  access_tier                = "Hot"
  account_tier               = "Standard"
  private_endpoint_subnet_id = data.azurerm_subnet.snet_private_endpoints.id
  ip_rules                   = local.ip_restrictions_as_string
  prevent_deletion           = true
  role_assignments = [
    {
      principal_id         = data.azurerm_client_config.current.object_id
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
}
