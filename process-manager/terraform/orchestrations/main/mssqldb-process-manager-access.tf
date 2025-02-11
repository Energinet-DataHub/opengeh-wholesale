module "mssql_database_access_process_manager" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database-application-access?ref=mssql-database-application-access_6.0.0"

  sql_server_name = data.azurerm_key_vault_secret.mssqldb_server_name.value
  database_name   = data.azurerm_key_vault_secret.mssqldb_name.value
  application_hosts_names = [
    module.func_orchestrations.name,
  ]
  depends_on = [
    module.func_orchestrations.name,
  ]
}
