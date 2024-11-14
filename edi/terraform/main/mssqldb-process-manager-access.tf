module "mssql_database_process_manager_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=mssql-database-application-access_5.0.0"

  sql_server_name         = module.mssqldb_process_manager.server_name
  database_name           = module.mssqldb_process_manager.name
  application_hosts_names = [
    module.func_process_manager_orchestrations.name,
    module.func_process_manager_api.name,
  ]
  depends_on = [
    module.func_process_manager_orchestrations.name,
    module.func_process_manager_api.name,
  ]
}
