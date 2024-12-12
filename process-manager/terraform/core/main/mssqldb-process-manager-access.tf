module "mssql_database_access_process_manager" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=mssql-database-application-access_5.0.0"

  sql_server_name = module.mssqldb_process_manager.server_name
  database_name   = module.mssqldb_process_manager.name
  application_hosts_names = [
    module.func_api.name,
  ]
  depends_on = [
    module.func_api.name,
  ]
}
