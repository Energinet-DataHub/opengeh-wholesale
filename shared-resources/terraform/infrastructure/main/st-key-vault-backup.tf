module "st_key_vault_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=14.0.3"

  name                       = "kvbackup"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  access_tier                = "Hot"
  private_endpoint_subnet_id = data.azurerm_subnet.snet_private_endpoints.id
  ip_rules                   = local.ip_restrictions_as_string
  prevent_deletion           = true
  role_assignments = [
    {
      principal_id         = data.azurerm_client_config.current.object_id
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
  blob_storage_backup_policy = {
    backup_policy_id          = module.backup_vault.blob_storage_backup_policy_id
    backup_vault_id           = module.backup_vault.id
    backup_vault_location     = azurerm_resource_group.this.location
    backup_vault_principal_id = module.backup_vault.identity.0.principal_id
  }
}

# Automatically delete storage account blobs and snapshots after 22 days according to the retention policy
resource "azurerm_storage_management_policy" "retention" {
  storage_account_id = module.st_key_vault_backup.id

  rule {
    name    = "retention"
    enabled = true
    filters {
      blob_types = ["blockBlob"]
    }
    actions {
      base_blob {
        delete_after_days_since_creation_greater_than = 22
      }
      snapshot {
        delete_after_days_since_creation_greater_than = 22
      }
      version {
        delete_after_days_since_creation = 22
      }
    }
  }
}
