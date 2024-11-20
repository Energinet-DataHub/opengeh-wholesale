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
