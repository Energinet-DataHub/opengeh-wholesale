module "dbw" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-workspace?ref=v13"

  name                                     = "dbw"
  project_name                             = var.domain_name_short
  environment_short                        = var.environment_short
  environment_instance                     = var.environment_instance
  resource_group_name                      = azurerm_resource_group.this.name
  location                                 = azurerm_resource_group.this.location
  sku                                      = "premium"
  main_virtual_network_id                  = data.azurerm_key_vault_secret.main_virtual_network_id.value
  main_virtual_network_name                = data.azurerm_key_vault_secret.main_virtual_network_name.value
  main_virtual_network_resource_group_name = data.azurerm_key_vault_secret.main_virtual_network_resource_group_name.value
  databricks_virtual_network_address_space = var.databricks_vnet_address_space
  private_subnet_address_prefix            = var.databricks_private_subnet_address_prefix
  public_subnet_address_prefix             = var.databricks_public_subnet_address_prefix
  private_endpoints_subnet_address_prefix  = var.databricks_private_endpoints_subnet_address_prefix
  user_access_security_group_object_id     = var.developers_security_group_object_id

  public_network_service_endpoints = [
    "Microsoft.EventHub"
  ]

  private_dns_records = [
    {
      resource_name = data.azurerm_key_vault_secret.st_data_lake_name.value
      dns_zone      = "privatelink.blob.core.windows.net"
      ip_record     = data.azurerm_key_vault_secret.st_data_lake_blob_private_ip_address.value
    },
    {
      resource_name = data.azurerm_key_vault_secret.st_data_lake_name.value
      dns_zone      = "privatelink.dfs.core.windows.net"
      ip_record     = data.azurerm_key_vault_secret.st_data_lake_dfs_private_ip_address.value
    },
    {
      resource_name = module.st_dh2data.name
      dns_zone      = "privatelink.blob.core.windows.net"
      ip_record     = module.st_dh2data.blob_private_ip_address
    },
    {
      resource_name = module.st_dh2data.name
      dns_zone      = "privatelink.dfs.core.windows.net"
      ip_record     = module.st_dh2data.dfs_private_ip_address
    },
    {
      resource_name = module.st_migrations.name
      dns_zone      = "privatelink.blob.core.windows.net"
      ip_record     = module.st_migrations.blob_private_ip_address
    },
    {
      resource_name = module.st_migrations.name
      dns_zone      = "privatelink.dfs.core.windows.net"
      ip_record     = module.st_migrations.dfs_private_ip_address
    },
    {
      resource_name = module.st_dh2dropzone_archive.name
      dns_zone      = "privatelink.blob.core.windows.net"
      ip_record     = module.st_dh2dropzone_archive.blob_private_ip_address
    },
    {
      resource_name = module.st_dh2dropzone_archive.name
      dns_zone      = "privatelink.dfs.core.windows.net"
      ip_record     = module.st_dh2dropzone_archive.dfs_private_ip_address
    },
    {
      resource_name = module.st_dh2dropzone.name
      dns_zone      = "privatelink.blob.core.windows.net"
      ip_record     = module.st_dh2dropzone.blob_private_ip_address
    },
    {
      resource_name = module.st_dh2dropzone.name
      dns_zone      = "privatelink.dfs.core.windows.net"
      ip_record     = module.st_dh2dropzone.dfs_private_ip_address
    }
  ]
}

resource "databricks_git_credential" "ado" {
  provider              = databricks.dbw
  git_username          = var.github_username
  git_provider          = "gitHub"
  personal_access_token = var.github_personal_access_token
  depends_on            = [module.dbw]
}

module "kvs_databricks_workspace_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "dbw-workspace-id"
  value        = module.dbw.id
  key_vault_id = module.kv_internal.id
  depends_on   = [module.dbw]
}

module "kvs_databricks_public_network_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "dbw-public-network-id"
  value        = module.dbw.public_network_id
  key_vault_id = module.kv_internal.id
  depends_on   = [module.dbw]
}

module "kvs_databricks_private_dns_resource_group_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "databricks-private-dns-resource-group-name"
  value        = module.dbw.private_dns_zone_resource_group_name
  key_vault_id = module.kv_internal.id
  depends_on   = [module.dbw]
}

module "kvs_databricks_dbw_workspace_token" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "dbw-workspace-token"
  value        = module.dbw.databricks_token
  key_vault_id = module.kv_internal.id
  depends_on   = [module.dbw]
}

resource "azurerm_monitor_diagnostic_setting" "dbw_diagnostic_settings" {
  name                       = "dbw-diagnostic-settings"
  target_resource_id         = module.dbw.id
  log_analytics_workspace_id = data.azurerm_key_vault_secret.log_shared_id.value

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
