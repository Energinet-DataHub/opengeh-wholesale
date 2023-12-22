module "mssql_database_application_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=v13"

  sql_server_name = data.azurerm_mssql_server.mssqlsrv.name
  database_name   = module.mssqldb_edi.name
  application_hosts_names = [
    module.func_receiver.name,
    module.b2c_web_api.name,
  ]
  depends_on = [
    module.func_receiver.name,
    module.b2c_web_api.name,
  ]
}
