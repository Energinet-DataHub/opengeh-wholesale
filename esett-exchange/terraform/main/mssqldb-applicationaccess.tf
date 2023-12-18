module "mssql_database_application_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=v13"

  sql_server_name = data.azurerm_mssql_server.mssqlsrv.name
  database_name   = module.mssqldb_esett_exchange.name

  application_hosts_names = [
    module.func_entrypoint_exchange_event_receiver.name,
    module.func_entrypoint_ecp_inbox.name,
    module.func_entrypoint_ecp_outbox.name,
    module.app_webapi.name,
  ]

  depends_on = [
    module.func_entrypoint_exchange_event_receiver.name,
    module.func_entrypoint_ecp_inbox.name,
    module.func_entrypoint_ecp_outbox.name,
    module.app_webapi.name,
  ]
}
