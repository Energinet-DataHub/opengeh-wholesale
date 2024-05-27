module "dbw" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-workspace?ref=14.12.0"
  providers = { # The databricks module requires a databricks provider, as it uses databricks resources
    databricks = databricks.dbw
  }

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
      resource_name = module.st_data_wholesale.name
      dns_zone      = "privatelink.blob.core.windows.net"
      ip_record     = module.st_data_wholesale.blob_private_ip_address
    },
    {
      resource_name = module.st_data_wholesale.name
      dns_zone      = "privatelink.dfs.core.windows.net"
      ip_record     = module.st_data_wholesale.dfs_private_ip_address
    }
  ]
}

module "kvs_databricks_workspace_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "dbw-workspace-id"
  value        = module.dbw.id
  key_vault_id = module.kv_internal.id
}

module "kvs_databricks_workspace_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "dbw-workspace-url"
  value        = module.dbw.workspace_url
  key_vault_id = module.kv_internal.id
}

module "kvs_databricks_dbw_workspace_token" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "dbw-workspace-token"
  value        = module.dbw.databricks_token
  key_vault_id = module.kv_internal.id
}
