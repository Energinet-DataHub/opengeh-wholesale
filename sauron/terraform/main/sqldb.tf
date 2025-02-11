module "mssqldb" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=mssql-database_10.0.0"

  name                 = "data"
  location             = azurerm_resource_group.this.location
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance

  server = {
    name                = data.azurerm_key_vault_secret.mssql_data_name.value
    resource_group_name = data.azurerm_key_vault_secret.mssql_data_resource_group_name.value
  }

  # Find available SKU's for "single database" (not pooled) here: https://learn.microsoft.com/en-us/azure/azure-sql/database/resource-limits-vcore-single-databases?view=azuresql
  sku_name                    = "GP_S_Gen5_6" # General Purpose (GP) - serverless compute (S) - standard series (Gen5) - max vCores (<number>) : https://learn.microsoft.com/en-us/azure/azure-sql/database/resource-limits-vcore-single-databases?view=azuresql#gen5-hardware-part-1-1
  min_capacity                = 1.5           #
  max_size_gb                 = 20
  auto_pause_delay_in_minutes = -1

  monitor_action_group = {
    id                  = module.monitor_action_group_sauron.id
    resource_group_name = azurerm_resource_group.this.name
  }
}

module "mssql_database_application_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=mssql-database-application-access_6.0.0"

  sql_server_name = module.mssqldb.server_name
  database_name   = module.mssqldb.name

  application_hosts_names = [
    module.func_github_api.name,
    module.func_bff_api.name,
    module.func_github_api.slot_name,
    module.func_bff_api.slot_name
  ]

  depends_on = [
    module.func_github_api.name,
    module.func_bff_api.name,
    module.func_github_api.slot_name,
    module.func_bff_api.slot_name
  ]
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
