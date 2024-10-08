module "mssql_database_application_access" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=mssql-database-application-access_5.0.0"

  sql_server_name = module.mssqldb_dh2_bridge.server_name
  database_name   = module.mssqldb_dh2_bridge.name

  application_hosts_names = [
    module.func_entrypoint_grid_loss_sender.name,
    module.func_entrypoint_grid_loss_peek.name,
    module.func_entrypoint_grid_loss_event_receiver.name,
  ]

  depends_on = [
    module.func_entrypoint_grid_loss_event_receiver.name,
    module.func_entrypoint_grid_loss_sender.name,
    module.func_entrypoint_grid_loss_peek.name,
  ]
}
