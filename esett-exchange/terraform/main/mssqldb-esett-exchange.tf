data "azurerm_mssql_server" "mssqlsrv" {
  name                = data.azurerm_key_vault_secret.mssql_data_name.value
  resource_group_name = var.shared_resources_resource_group_name
}

module "mssqldb_esett_exchange" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=v11"

  name                               = "esett-exchange"
  project_name                       = var.domain_name_short
  environment_short                  = var.environment_short
  environment_instance               = var.environment_instance
  server_id                          = data.azurerm_mssql_server.mssqlsrv.id
  log_analytics_workspace_id         = data.azurerm_key_vault_secret.log_shared_id.value
  sql_server_name                    = data.azurerm_mssql_server.mssqlsrv.name
  elastic_pool_id                    = data.azurerm_key_vault_secret.mssql_data_elastic_pool_id.value
  monitor_alerts_action_group_id     = data.azurerm_key_vault_secret.primary_action_group_id.value
  monitor_alerts_resource_group_name = azurerm_resource_group.this.name
}

module "kvs_sql_ms_esett_exchange_database_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v11"

  name         = "mssql-esett-exchange-database-name"
  value        = module.mssqldb_esett_exchange.name
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
