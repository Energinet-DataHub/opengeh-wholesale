module "mssql_database_application_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=mssql-database-application-access_6.0.0"

  sql_server_name = module.mssqldb_market_participant.server_name
  database_name   = module.mssqldb_market_participant.name
  application_hosts_names = [
    module.app_api.name,
    module.app_api.slot_name,
    module.func_organization.name,
  ]

  depends_on = [
    module.app_api.name,
    module.func_organization.name,
  ]
}
