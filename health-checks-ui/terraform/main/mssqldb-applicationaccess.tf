module "mssql_database_application_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=v12"

  sql_server_name = data.azurerm_mssql_server.mssqlsrv.name
  database_name   = module.mssqldb_health_checks_ui.name

  application_hosts_names = [
    module.app_health_checks_ui.name,
  ]

  depends_on = [
    module.app_health_checks_ui.name,
  ]
}
