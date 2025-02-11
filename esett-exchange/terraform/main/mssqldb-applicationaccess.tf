module "mssql_database_application_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=mssql-database-application-access_6.0.0"

  sql_server_name = module.mssqldb_esett_exchange.server_name
  database_name   = module.mssqldb_esett_exchange.name

  application_hosts_names = [
    module.func_entrypoint_exchange_event_receiver.name,
    module.func_entrypoint_ecp_inbox.name,
    module.func_entrypoint_ecp_outbox.name,
    module.func_entrypoint_application_workers.name,
    module.app_webapi.name,
    module.app_webapi.slot_name,
  ]

  depends_on = [
    module.func_entrypoint_exchange_event_receiver.name,
    module.func_entrypoint_ecp_inbox.name,
    module.func_entrypoint_ecp_outbox.name,
    module.func_entrypoint_application_workers.name,
    module.app_webapi.name,
  ]
}
