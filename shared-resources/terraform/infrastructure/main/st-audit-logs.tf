# Resource group created for audit logs, as no one should be able to write to this storage account
resource "azurerm_resource_group" "audit_logs" {
  name     = "rg-audit${local.resources_suffix}"
  location = var.location

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  tags = local.tags
}

module "st_audit_logs" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=storage-account_7.1.1"

  name                       = "audit"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.audit_logs.name
  location                   = azurerm_resource_group.audit_logs.location
  access_tier                = "Hot"
  private_endpoint_subnet_id = data.azurerm_subnet.snet_private_endpoints_002.id
  ip_rules                   = local.ip_restrictions_as_string
  audit_storage_account      = null # We disable audit logging on the audit storage account
  role_assignments = [
    {
      principal_id         = data.azurerm_client_config.current.object_id
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
  blob_storage_backup_policy = {
    backup_policy_id          = module.backup_vault.blob_storage_backup_vaulted_policy_id
    backup_vault_id           = module.backup_vault.id
    backup_vault_location     = azurerm_resource_group.this.location
    backup_vault_principal_id = module.backup_vault.identity.0.principal_id
  }
}

module "kvs_st_audit_shres_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-audit-shres-name"
  value        = module.st_audit_logs.name
  key_vault_id = module.kv_shared.id
}

module "kvs_st_audit_shres_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-audit-shres-id"
  value        = module.st_audit_logs.id
  key_vault_id = module.kv_shared.id
}

# Automatically delete storage account blobs and snapshots after 180 days according to the retention policy for audit
resource "azurerm_storage_management_policy" "retention_audit" {
  storage_account_id = module.st_audit_logs.id

  rule {
    name    = "retention"
    enabled = true
    filters {
      blob_types = ["blockBlob", "appendBlob"]
    }
    actions {
      base_blob {
        delete_after_days_since_creation_greater_than = 180
      }
      snapshot {
        delete_after_days_since_creation_greater_than = 180
      }
      version {
        delete_after_days_since_creation = 180
      }
    }
  }
}

module "pim_reader_security_group_permissions_audit" {
  count = var.pim_reader_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_6.0.1"

  resource_group_name = azurerm_resource_group.audit_logs.name
  security_group_name = var.pim_reader_group_name
  role_level          = "Reader"

  depends_on = [azurerm_resource_group.audit_logs]
}

# Only applied to dev environments
module "developer_security_group_permissions_reader_audit" {
  count = var.developer_security_group_reader_access == true ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_6.0.1"

  resource_group_name = azurerm_resource_group.audit_logs.name
  security_group_name = var.developer_security_group_name
  role_level          = "Reader"

  depends_on = [azurerm_resource_group.audit_logs]
}

module "platform_security_group_permissions_reader_audit" {
  count = var.platform_security_group_reader_access == true ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_6.0.1"

  resource_group_name = azurerm_resource_group.audit_logs.name
  security_group_name = var.platform_security_group_name
  role_level          = "Reader"

  depends_on = [azurerm_resource_group.audit_logs]
}
