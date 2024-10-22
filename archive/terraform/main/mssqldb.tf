module "mssqldb_this" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=mssql-database_8.0.1"

  name                 = "archive"
  location             = azurerm_resource_group.this.location
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance

  server = {
    name                = module.mssql_this.name
    resource_group_name = azurerm_resource_group.this.name
  }

  sku_name                    = "GP_S_Gen5_2" # General Purpose (GP) - serverless compute (S) - standard series (Gen5) - max vCores (<number>) : https://learn.microsoft.com/en-us/azure/azure-sql/database/resource-limits-vcore-single-databases?view=azuresql#gen5-hardware-part-1-1
  min_capacity                = 1
  max_size_gb                 = 1
  auto_pause_delay_in_minutes = -1

  monitor_action_group = length(module.monitor_action_group) != 1 ? null : {
    id                  = module.monitor_action_group[0].id
    resource_group_name = azurerm_resource_group.this.name
  }
}
