module "mssql_database_application_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=mssql-database-application-access_5.0.0"

  sql_server_name = module.mssqldb_edi.server_name
  database_name   = module.mssqldb_edi.name
  application_hosts_names = [
    # module.func_receiver.name,
    module.b2c_web_api.name,
    module.b2c_web_api.slot_name,
  ]
  depends_on = [
    # module.func_receiver.name,
    module.b2c_web_api.name,
  ]
}
