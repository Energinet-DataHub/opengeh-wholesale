data "azurerm_mssql_server" "mssqlsrv" {
  name                = data.azurerm_key_vault_secret.mssql_data_name.value
  resource_group_name = var.shared_resources_resource_group_name
}

module "mssqldb" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=14.8.2"

  name                 = "data"
  location             = azurerm_resource_group.this.location
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  server_id            = data.azurerm_mssql_server.mssqlsrv.id
  sql_server_name      = data.azurerm_mssql_server.mssqlsrv.name
  elastic_pool_id      = data.azurerm_key_vault_secret.mssql_data_elastic_pool_id.value
  monitor_action_group = {
    id                  = module.monitor_action_group_sauron.id
    resource_group_name = azurerm_resource_group.this.name
  }
}

module "mssql_database_application_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=14.7.1"

  sql_server_name = data.azurerm_mssql_server.mssqlsrv.name
  database_name   = module.mssqldb.name

  application_hosts_names = [
    module.func_github_api.name,
    module.func_bff_api.name
  ]

  depends_on = [
    module.func_github_api.name,
    module.func_bff_api.name
  ]
}

locals {
  pim_security_group_rules_001 = [
    {
      name = var.pim_sql_reader_ad_group_name
    },
    {
      name                 = var.pim_sql_writer_ad_group_name
      enable_db_datawriter = true
    }
  ]
  developer_security_group_rules_001_dev_test = [
    {
      name = var.omada_developers_security_group_name
    }
  ]
  developer_security_group_rules_002 = [
    {
      name                 = var.omada_developers_security_group_name
      enable_db_datawriter = true
    }
  ]
}
