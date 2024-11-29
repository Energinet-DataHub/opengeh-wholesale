data "azurerm_client_config" "this" {}

module "st_this" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=storage-account_7.1.0"

  name                       = "docs"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  access_tier                = "Cool"
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  ip_rules                   = local.ip_restrictions_as_string
  role_assignments = [
    {
      principal_id         = data.azurerm_client_config.this.object_id
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
  containers = [
    {
      name = "esett-deprecated"
    },
    {
      name = "esett-deprecated-ccoe-sub"
    },
  ]

  audit_storage_account = var.enable_audit_logs ? {
    id = data.azurerm_key_vault_secret.st_audit_shres_id.value
  } : null

  blob_storage_backup_policy = {
      backup_policy_id          = data.azurerm_key_vault_secret.bvault_vaulted_policy_id.value
      backup_vault_id           = data.azurerm_key_vault_secret.bvault_vault_id.value
      backup_vault_location     = data.azurerm_key_vault_secret.bvault_vault_location.value
      backup_vault_principal_id = data.azurerm_key_vault_secret.bvault_vault_principal_id.value
  }
}
