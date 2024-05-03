module "mssql_database_application_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=v14"

  sql_server_name = data.azurerm_mssql_server.mssqlsrv.name
  database_name   = module.mssqldb_wholesale.name
  application_hosts_names = [
    module.app_wholesale_api.name,
    module.func_wholesale_orchestrations.name,
  ]

  depends_on = [
    module.app_wholesale_api.name,
    module.func_wholesale_orchestrations.name,
  ]
}
