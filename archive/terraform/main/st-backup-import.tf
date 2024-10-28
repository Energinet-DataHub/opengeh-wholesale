module "st_backup_import" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=storage-account_6.2.0"

  name                       = "import"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  access_tier                = "Hot"
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_private_endpoints_002_id.value
  ip_rules                   = local.ip_restrictions_as_string
  role_assignments = [
    {
      principal_id         = data.azurerm_client_config.this.object_id
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
  containers = [
    {
      name = "import"
    },
  ]

  audit_storage_account = null
}
