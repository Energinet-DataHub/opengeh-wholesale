data "azurerm_mssql_server" "mssqlsrv" {
  name                = data.azurerm_key_vault_secret.mssql_data_name.value
  resource_group_name = data.azurerm_resource_group.shared.name
}

module "mssqldb_grid_loss_imbalance_prices" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=14.7.1"

  name                               = "grid-loss-imbalance-prices"
  location                           = azurerm_resource_group.this.location
  project_name                       = var.domain_name_short
  environment_short                  = var.environment_short
  environment_instance               = var.environment_instance
  server_id                          = data.azurerm_mssql_server.mssqlsrv.id
  sql_server_name                    = data.azurerm_mssql_server.mssqlsrv.name
  elastic_pool_id                    = data.azurerm_key_vault_secret.mssql_data_elastic_pool_id.value
  monitor_alerts_action_group_id     = data.azurerm_key_vault_secret.primary_action_group_id.value
  monitor_alerts_resource_group_name = azurerm_resource_group.this.name
}

module "kvs_sql_ms_grid_loss_imbalance_prices_database_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "mssql-grid-loss-imbalance-prices-database-name"
  value        = module.mssqldb_grid_loss_imbalance_prices.name
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
      name = var.developer_ad_group_name
    },
    {
      name = var.omada_developers_security_group_name
    }
  ]
  developer_security_group_rules_002 = [
    {
      name                 = var.developer_ad_group_name
      enable_db_datawriter = true
    },
    {
      name                 = var.omada_developers_security_group_name
      enable_db_datawriter = true
    }
  ]
}
