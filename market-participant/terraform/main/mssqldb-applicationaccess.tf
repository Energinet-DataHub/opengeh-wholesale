module "mssql_database_application_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=v13"

  sql_server_name = data.azurerm_mssql_server.mssqlsrv.name
  database_name   = module.mssqldb_market_participant.name
  application_hosts_names = [
    module.app_webapi.name,
    module.func_entrypoint_marketparticipant.name,
  ]

  depends_on = [
    module.app_webapi.name,
    module.func_entrypoint_marketparticipant.name,
  ]
}
