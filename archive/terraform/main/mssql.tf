module "mssql_this" { # Needs to be a named like this or it would delete all databases
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-server?ref=mssql-server_7.1.0"

  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  sql_version          = "12.0"
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  monitor_action_group = {
    id                  = module.monitor_action_group.id
    resource_group_name = azurerm_resource_group.this.name
  }

  ad_group_directory_reader = var.ad_group_directory_reader

  elastic_pool_max_size_gb      = 100
  public_network_access_enabled = true

  # If using DTU model then see pool limits based on SKU here: https://learn.microsoft.com/en-us/azure/azure-sql/database/resource-limits-dtu-elastic-pools?view=azuresql#standard-elastic-pool-limits
  elastic_pool_sku = {
    name     = "StandardPool"
    tier     = "Standard"
    capacity = 200
  }

  elastic_pool_per_database_settings = {
    min_capacity = 0
    max_capacity = 100
  }

  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  audit_storage_account = var.enable_audit_logs ? {
    id                    = data.azurerm_key_vault_secret.st_audit_shres_id.value
    primary_blob_endpoint = "https://${data.azurerm_key_vault_secret.st_audit_shres_name.value}.blob.core.windows.net/"
  } : null
}

resource "azurerm_mssql_firewall_rule" "github_largerunner" {
  count = length(split(",", local.ip_restrictions_as_string))

  name             = "github_largerunner_${count.index}"
  server_id        = module.mssql_this.id
  start_ip_address = cidrhost(split(",", local.ip_restrictions_as_string)[count.index], 0)  #First IP in range
  end_ip_address   = cidrhost(split(",", local.ip_restrictions_as_string)[count.index], -1) #Last IP in range
}
