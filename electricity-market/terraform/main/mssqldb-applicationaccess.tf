module "mssql_database_application_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=mssql-database-application-access_5.0.0"

  sql_server_name = module.mssqldb_electricity_market.server_name
  database_name   = module.mssqldb_electricity_market.name
  application_hosts_names = [
    module.app_api.name,
    module.app_api.slot_name,
  ]

  depends_on = [
    module.app_api.name,
  ]
}

module "mssql_markpart_application_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-access?ref=mssql-database-access_1.0.1"

  sql_server_name = data.azurerm_key_vault_secret.mssql_data_name.value
  database_name   = data.azurerm_key_vault_secret.markpart_db_name.value
  schema_name     = "electricitymarket"
  write_access    = true

  principal_names = [
    module.app_api.name,
    module.app_api.slot_name,
    module.func_import.name,
  ]

  depends_on = [
    module.app_api,
    module.func_import
  ]
}
