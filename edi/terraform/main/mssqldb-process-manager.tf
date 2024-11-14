module "mssqldb_process_manager" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=mssql-database_9.0.1"

  name                 = "process-manager"
  location             = azurerm_resource_group.this.location
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance

  server = {
    name                = data.azurerm_key_vault_secret.mssql_data_name.value
    resource_group_name = data.azurerm_key_vault_secret.mssql_data_resource_group_name.value
  }

  elastic_pool = {
    name                = data.azurerm_key_vault_secret.mssql_data_elastic_pool_name.value
    resource_group_name = data.azurerm_key_vault_secret.mssql_data_elastic_pool_resource_group_name.value
  }

  monitor_action_group  = length(module.monitor_action_group_edi) != 1 ? null : {
    id                  = module.monitor_action_group_edi[0].id
    resource_group_name = azurerm_resource_group.this.name
  }
}

module "kvs_process_manager_sql_database_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "mssql-pm-database-name"
  value        = module.mssqldb_process_manager.name
  key_vault_id = module.kv_internal.id
}

module "kvs_process_manager_sql_connection_string_db_migrations" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "mssql-pm-connection-string-db-migrations"
  value        = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_process_manager.name};Persist Security Info=False;Authentication=Active Directory Default;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  key_vault_id = module.kv_internal.id
}
