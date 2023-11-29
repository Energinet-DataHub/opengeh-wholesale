data "azurerm_mssql_server" "mssqlsrv" {
  name                = data.azurerm_key_vault_secret.mssql_data_name.value
  resource_group_name = var.shared_resources_resource_group_name
}

module "mssqldb_esett" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=v13"

  name                               = "data"
  location                           = azurerm_resource_group.this.location
  project_name                       = var.domain_name_short
  environment_short                  = var.environment_short
  environment_instance               = var.environment_instance
  server_id                          = data.azurerm_mssql_server.mssqlsrv.id
  sql_server_name                    = data.azurerm_mssql_server.mssqlsrv.name
  elastic_pool_id                    = data.azurerm_key_vault_secret.mssql_data_elastic_pool_id.value
  monitor_alerts_action_group_id     = data.azurerm_key_vault_secret.ag_primary_id.value
  monitor_alerts_resource_group_name = azurerm_resource_group.this.name
}

module "mssql_database_application_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=v13"

  sql_server_name = data.azurerm_mssql_server.mssqlsrv.name
  database_name   = module.mssqldb_esett.name

  application_hosts_names = [
    module.app_importer.name,
    module.func_biztalkshipper.name,
    module.func_biztalkreceiver.name,
    module.func_changeobserver.name,
    module.func_converter.name,
    azuread_application.app_powerbi.display_name,
  ]
}
