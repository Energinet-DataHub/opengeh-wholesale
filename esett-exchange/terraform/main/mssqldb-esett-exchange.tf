module "mssqldb_esett_exchange" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=mssql-database_9.0.1"

  name                 = "esett-exchange"
  enclave_type         = null
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

  monitor_action_group = length(module.monitor_action_group_esett) != 1 ? null : {
    id                  = module.monitor_action_group_esett[0].id
    resource_group_name = azurerm_resource_group.this.name
  }

  max_size_gb = 5
}

module "kvs_sql_ms_esett_exchange_database_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "mssql-esett-exchange-database-name"
  value        = module.mssqldb_esett_exchange.name
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

module "kvs_sql_ms_esett_exchange_connection_string" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "mssql-esett-exchange-connection-string"
  value        = local.CONNECTION_STRING_DB_MIGRATIONS
  key_vault_id = module.kv_internal.id
}

locals {
  pim_security_group_rules_001 = [
    {
      name = var.pim_reader_group_name
    },
    {
      name                 = var.pim_contributor_data_plane_group_name
      enable_db_datawriter = true
    }
  ]
  developer_security_group_rules_001_dev_test = [
    {
      name = var.developer_security_group_name
    }
  ]
  developer_security_group_rules_002 = [
    {
      name                 = var.developer_security_group_name
      enable_db_datawriter = true
    }
  ]
}
