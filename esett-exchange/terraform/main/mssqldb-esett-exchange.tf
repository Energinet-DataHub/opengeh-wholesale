data "azurerm_mssql_server" "mssqlsrv" {
  name                = data.azurerm_key_vault_secret.mssql_data_name.value
  resource_group_name = data.azurerm_resource_group.shared.name
}

module "mssqldb_esett_exchange" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=14.8.2"

  name                 = "esett-exchange"
  location             = azurerm_resource_group.this.location
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  server_id            = data.azurerm_mssql_server.mssqlsrv.id
  sql_server_name      = data.azurerm_mssql_server.mssqlsrv.name
  elastic_pool_id      = data.azurerm_key_vault_secret.mssql_data_elastic_pool_id.value
  monitor_action_group = {
    id                  = data.azurerm_key_vault_secret.primary_action_group_id.value
    resource_group_name = azurerm_resource_group.this.name
  }
  max_size_gb = 5
}

module "kvs_sql_ms_esett_exchange_database_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "mssql-esett-exchange-database-name"
  value        = module.mssqldb_esett_exchange.name
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
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
