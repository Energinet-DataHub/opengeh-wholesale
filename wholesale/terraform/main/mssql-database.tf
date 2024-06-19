data "azurerm_mssql_server" "mssqlsrv" {
  name                = data.azurerm_key_vault_secret.mssql_data_name.value
  resource_group_name = data.azurerm_resource_group.shared.name
}

module "mssqldb_wholesale" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=14.19.1"

  name                 = "data"
  enclave_type         = null
  location             = azurerm_resource_group.this.location
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  server_id            = data.azurerm_mssql_server.mssqlsrv.id
  sql_server_name      = data.azurerm_mssql_server.mssqlsrv.name
  elastic_pool_id      = data.azurerm_key_vault_secret.mssql_data_elastic_pool_id.value

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
      name                 = var.pim_contributor_group_name
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
