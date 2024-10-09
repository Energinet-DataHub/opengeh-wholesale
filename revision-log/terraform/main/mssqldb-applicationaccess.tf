module "create_hosts_as_db_readers_or_writers" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=mssql-database-application-access_5.0.0"

  sql_server_name = module.mssqldb_revision_log.server_name
  database_name   = module.mssqldb_revision_log.name

  application_hosts_names = ["", ""]

  roles_with_application_access = [
    {
      role_name               = "db_revision_log_reader",
      application_hosts_names = [module.app_reader_api.name]
    },
    {
      role_name               = "db_revision_log_writer",
      application_hosts_names = [module.func_log_ingestion.name]
    }
  ]
}
