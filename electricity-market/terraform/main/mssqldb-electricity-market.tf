module "mssqldb_electricity_market" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=mssql-database_9.0.1"

  name                 = "electricity-market"
  location             = azurerm_resource_group.this.location
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance

  server = {
    name                = data.azurerm_key_vault_secret.mssql_data_name.value
    resource_group_name = data.azurerm_key_vault_secret.mssql_data_resource_group_name.value
  }

  # Find available SKU's for "single database" (not pooled) here: https://learn.microsoft.com/en-us/azure/azure-sql/database/resource-limits-vcore-single-databases?view=azuresql
  sku_name                    = "HS_S_Gen5_4" # Hyperscale (HS) - serverless - standard series (Gen5) - max vCores (<number>) : https://learn.microsoft.com/en-us/azure/azure-sql/database/resource-limits-vcore-single-databases?view=azuresql#gen5-hardware-part-1-1
  max_size_gb                 = null          # Auto scaling by hyperscale (between 10GB and 128TB)
  auto_pause_delay_in_minutes = -1            # Auto-pause is not supported for hyperscale
  min_capacity                = 1.5
  short_term_retention_policy = {
    retention_days           = 22
    backup_interval_in_hours = null # Managed by Hyperscale, see https://learn.microsoft.com/en-us/azure/azure-sql/database/hyperscale-automated-backups-overview?view=azuresql#backup-scheduling
  }

  # Storage metric doesn't exist for hyperscale, so the module is not ready to support metrics for hyperscale
  # Issue at Outlaws: https://app.zenhub.com/workspaces/the-outlaws-6193fe815d79fc0011e741b1/issues/gh/energinet-datahub/team-the-outlaws/2581
  # monitor_action_group = length(module.monitor_action_group_elmk) != 1 ? null : {
  #   id                  = module.monitor_action_group_elmk[0].id
  #   resource_group_name = azurerm_resource_group.this.name
  # }
}

module "kvs_sql_ms_electricity_market_database_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "mssql-electricity-market-database-name"
  value        = module.mssqldb_electricity_market.name
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
