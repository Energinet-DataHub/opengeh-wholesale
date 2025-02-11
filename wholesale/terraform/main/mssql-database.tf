module "mssqldb_wholesale" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=mssql-database_10.0.0"

  name                 = "data"
  enclave_type         = null
  location             = azurerm_resource_group.this.location
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance

  server = {
    name                = data.azurerm_key_vault_secret.mssql_data_name.value
    resource_group_name = data.azurerm_key_vault_secret.mssql_data_resource_group_name.value
  }

  sku_name                    = var.mssql_sku_name
  min_capacity                = var.mssql_min_capacity_vcore
  max_size_gb                 = var.mssql_max_size_gb
  auto_pause_delay_in_minutes = -1

  monitor_action_group = length(module.monitor_action_group_wholesale) != 1 ? null : {
    id                  = module.monitor_action_group_wholesale[0].id
    resource_group_name = azurerm_resource_group.this.name
  }
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
