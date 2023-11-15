module "mssql_data_additional" { # Needs to be a new module as Terraform will NOT override depends_on - Delete it when old subscriptions are gone
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-server?ref=v13"

  name                 = "data"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  sql_version          = "12.0"
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  monitor_action_group = {
    name                = module.ag_primary.name
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
  private_endpoint_subnet_id = data.azurerm_subnet.snet_private_endpoints.id

  depends_on = [
    module.ag_primary
  ]
}
