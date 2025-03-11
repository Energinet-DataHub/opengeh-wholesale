module "dbw" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-workspace?ref=databricks-workspace_11.0.0"
  providers = { # The databricks module requires a databricks provider, as it uses databricks resources
    databricks = databricks.dbw
  }

  project_name                               = var.domain_name_short
  environment_short                          = var.environment_short
  environment_instance                       = var.environment_instance
  resource_group_name                        = azurerm_resource_group.this.name
  location                                   = azurerm_resource_group.this.location
  sku                                        = "premium"
  main_virtual_network_id                    = data.azurerm_key_vault_secret.main_virtual_network_id.value
  main_virtual_network_name                  = data.azurerm_key_vault_secret.main_virtual_network_name.value
  main_virtual_network_resource_group_name   = data.azurerm_key_vault_secret.main_virtual_network_resource_group_name.value
  databricks_virtual_network_address_space   = var.databricks_vnet_address_space
  private_subnet_address_prefix              = var.databricks_private_subnet_address_prefix
  public_subnet_address_prefix               = var.databricks_public_subnet_address_prefix
  catalog_name                               = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  enable_verbose_audit_logs                  = var.databricks_enable_verbose_audit_logs
  private_dns_resolver_forwarding_ruleset_id = data.azurerm_key_vault_secret.dns_resolver_forwarding_ruleset_id.value

  scim_databrick_group_ids = [
    var.databricks_readers_group.id,
    var.databricks_contributor_dataplane_group.id
  ]

  public_network_service_endpoints = [
    "Microsoft.EventHub"
  ]
}

#
# Place Databricks streaming jobs in internal key vault.
#

module "kva_databricks_jobs" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "dbw-workspace-jobs"
  value        = local.databricks_streaming_jobs_string
  key_vault_id = module.kv_internal.id
}

#
# Places Databricks secrets in internal key vault
#

module "kvs_databricks_workspace_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "dbw-workspace-id"
  value        = module.dbw.id
  key_vault_id = module.kv_internal.id
}

module "kvs_databricks_workspace_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "dbw-workspace-url"
  value        = "https://${module.dbw.workspace_url}"
  key_vault_id = module.kv_internal.id
}

module "kvs_databricks_dbw_workspace_token" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "dbw-workspace-token"
  value        = module.dbw.databricks_token
  key_vault_id = module.kv_internal.id
}

#
# Places Databricks secrets in shared key vault so other subsystems can use REST API or data.
#

module "kvs_shared_databricks_workspace_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "dbw-measurements-workspace-url"
  value        = "https://${module.dbw.workspace_url}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

module "kvs_shared_databricks_dbw_workspace_token" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "dbw-measurements-workspace-token"
  value        = module.dbw.databricks_token
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

resource "azurerm_monitor_diagnostic_setting" "ds_dbw_audit" {
  name               = "ds-dbw-audit"
  target_resource_id = module.dbw.id
  storage_account_id = data.azurerm_key_vault_secret.st_audit_shres_id.value

  enabled_log {
    category = "jobs"
  }

  enabled_log {
    category = "workspace"
  }

  enabled_log {
    category = "sqlanalytics"
  }

  enabled_log {
    category = "databrickssql"
  }
}
