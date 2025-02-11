module "mssql_database_application_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=mssql-database-application-access_6.0.0"

  sql_server_name = module.mssqldb_notifications.server_name
  database_name   = module.mssqldb_notifications.name

  application_hosts_names = [
    module.func_notifications_worker.name,
  ]

  depends_on = [
    module.func_notifications_worker.name,
  ]
}
