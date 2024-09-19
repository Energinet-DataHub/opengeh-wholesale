module "mssql_database_application_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=mssql-database-application-access_4.0.1"

  sql_server_name = data.azurerm_mssql_server.mssqlsrv.name
  database_name   = module.mssqldb_settlement_report.name
  application_hosts_names = [
    module.app_api.name,
    module.app_api.slot_name,
    module.func_settlement_reports_df.name,
    module.func_settlement_reports_light_df.name
  ]

  depends_on = [
    module.app_api.name,
    module.func_settlement_reports_df.name,
    module.func_settlement_reports_light_df.name,
  ]
}
