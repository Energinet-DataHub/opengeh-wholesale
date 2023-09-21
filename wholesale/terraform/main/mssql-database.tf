data "azurerm_mssql_server" "mssqlsrv" {
  name                = data.azurerm_key_vault_secret.mssql_data_name.value
  resource_group_name = data.azurerm_resource_group.shared_resources.name
}

module "mssqldb_wholesale" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=v12"

  name                               = "data"
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
