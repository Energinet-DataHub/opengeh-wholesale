locals {
  tables = {
    EnergyPerBrp = {
      table_name = "energy_per_brp"
    }
    EnergyPerEs = {
      table_name = "energy_per_es"
    }
    EnergyPerGa = { # name differs in test-002
      table_name = "energy_per_ga"
    }
    ExchangePerNeighborGa = {
      table_name = "exchange_per_neighbor_ga"
    }
    GridLossMeteringPointTimeSeries = {
      table_name = "grid_loss_metering_point_time_series"
    }
    MonthlyAmountsPerCharge = {
      table_name = "monthly_amounts_per_charge"
    }
    TotalMonthlyAmounts = {
      table_name = "total_monthly_amounts"
    }
  }
  backup_access_control = local.readers == {} ? [
    {
      group_name         = var.databricks_contributor_dataplane_group.name
      contributor_access = true
    }] : [
    {
      group_name         = var.databricks_contributor_dataplane_group.name
      contributor_access = true
    },
    {
      group_name         = var.databricks_readers_group.name
      contributor_access = false
  }]
}

resource "databricks_sql_endpoint" "back_up_warehouse" {
  provider             = databricks.dbw
  name                 = "SQL Endpoint for running Deep Clone back-ups"
  cluster_size         = "Medium"
  max_num_clusters     = 2
  auto_stop_mins       = 15
  warehouse_type       = "PRO"
  spot_instance_policy = "RELIABILITY_OPTIMIZED"

  depends_on = [module.dbw]
}

resource "databricks_permissions" "back_up_endpoint" {
  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.back_up_warehouse.id

  access_control {
    group_name       = var.databricks_contributor_dataplane_group.name
    permission_level = "CAN_MANAGE"
  }
  dynamic "access_control" {
    for_each = local.readers
    content {
      group_name       = access_control.key
      permission_level = "CAN_USE"
    }
  }

  depends_on = [module.dbw]
}

module "results_internal_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-storage-backup?ref=databricks-storage-backup_1.0.0"
  providers = { # The module requires a databricks provider, as it uses databricks resources
    databricks = databricks.dbw
  }

  backup_storage_account_name   = module.st_dbw_backup.name
  backup_container_name         = azurerm_storage_container.backup_results_internal.name
  unity_storage_credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  shared_unity_catalog_name     = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  backup_schema_name            = "${databricks_schema.results_internal.name}_backup"
  backup_schema_comment         = databricks_schema.results_internal.comment
  tables                        = local.tables
  source_schema_name            = databricks_schema.results_internal.name
  backup_sql_endpoint_id        = databricks_sql_endpoint.back_up_warehouse.id
  access_control                = local.backup_access_control

  depends_on = [module.dbw]
}
