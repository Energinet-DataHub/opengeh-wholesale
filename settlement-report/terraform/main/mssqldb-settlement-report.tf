module "mssqldb_settlement_report" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=mssql-database_8.0.0"

  name                 = "settlement-report"
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

  monitor_action_group = length(module.monitor_action_group_setr) != 1 ? null : {
    id                  = module.monitor_action_group_setr[0].id
    resource_group_name = azurerm_resource_group.this.name
  }
}

module "kvs_sql_ms_settlement_report_database_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_5.0.0"

  name         = "mssql-settlement-report-database-name"
  value        = module.mssqldb_settlement_report.name
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
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
