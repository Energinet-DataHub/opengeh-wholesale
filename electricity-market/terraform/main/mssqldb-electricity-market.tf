module "mssqldb_electricity_market" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=mssql-database_9.0.1"

  name                 = "electricity-market"
  location             = azurerm_resource_group.this.location
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance

  server = {
    name                = data.azurerm_key_vault_secret.mssql_data_name.value
    resource_group_name = data.azurerm_key_vault_secret.mssql_data_resource_group_name.value
  }

  # Find available SKU's for "single database" (not pooled) here: https://learn.microsoft.com/en-us/azure/azure-sql/database/resource-limits-vcore-single-databases?view=azuresql
  sku_name                    = "HS_Gen5_4" # General Purpose (GP) - standard series (Gen5) - max vCores (<number>) : https://learn.microsoft.com/en-us/azure/azure-sql/database/resource-limits-vcore-single-databases?view=azuresql#gen5-hardware-part-1-1
  min_capacity                = 1.5
  max_size_gb                 = 10
  auto_pause_delay_in_minutes = -1

  monitor_action_group = length(module.monitor_action_group_elmk) != 1 ? null : {
    id                  = module.monitor_action_group_elmk[0].id
    resource_group_name = azurerm_resource_group.this.name
  }
}

module "kvs_sql_ms_electricity_market_database_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "mssql-electricity-market-database-name"
  value        = module.mssqldb_electricity_market.name
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

locals {
  pim_security_group_rules_001 = [
    {
      name = var.pim_reader_group_name
    },
    {
      name                 = var.pim_contributor_data_plane_group_name
      enable_db_datawriter = true
    }
  ]
  developer_security_group_rules_001_dev_test = [
    {
      name = var.developer_security_group_name
    }
  ]
  developer_security_group_rules_002 = [
    {
      name                 = var.developer_security_group_name
      enable_db_datawriter = true
    }
  ]
}
