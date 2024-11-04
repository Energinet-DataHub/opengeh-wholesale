locals {
  results_internal_schema = {
    AmountsPerCharge = {
      table_name = "amounts_per_charge"
    }
    Energy = {
      table_name = "energy"
    }
    EnergyPerBrp = {
      table_name = "energy_per_brp"
    }
    EnergyPerEs = {
      table_name = "energy_per_es"
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
  basis_data_internal_schema = {
    BasisDataInternalChargeLinkPeriods = {
      table_name = "charge_link_periods"
    }
    BasisDataInternalChargePriceInformationPeriods = {
      table_name = "charge_price_information_periods"
    }
    BasisDataInternalChargePricePoints = {
      table_name = "charge_price_points"
    }
    BasisDataInternalGridLossMeteringPoints = {
      table_name = "grid_loss_metering_points"
    }
    BasisDataInternalMeteringPointPeriods = {
      table_name = "metering_point_periods"
    }
    BasisDataInternalTimeSeriesPoints = {
      table_name = "time_series_points"
    }
  }
  internal_schema = {
    InternalCalculationGridAreas = {
      table_name = "calculation_grid_areas"
    }
    InternalCalculations = {
      table_name = "calculations"
    }
    InternalExecutedMigrations = {
      table_name = "executed_migrations"
    }
    InternalGridLossMeteringPoints = {
      table_name = "grid_loss_metering_points"
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
  warehouse_key = "backup_warehouse"
  warehouse_set = var.setup_backup_sql_warehouse == true ? toset([local.warehouse_key]) : toset([])
}

resource "databricks_sql_endpoint" "backup_warehouse" {
  for_each             = local.warehouse_set
  provider             = databricks.dbw
  name                 = "SQL Endpoint for running Deep Clone backups"
  cluster_size         = "Small"
  max_num_clusters     = 2
  auto_stop_mins       = 15
  warehouse_type       = "PRO"
  spot_instance_policy = "RELIABILITY_OPTIMIZED"

  depends_on = [module.dbw]
}

resource "databricks_permissions" "backup_endpoint" {
  for_each        = local.warehouse_set
  provider        = databricks.dbw
  sql_endpoint_id = databricks_sql_endpoint.backup_warehouse[each.key].id

  access_control {
    group_name       = var.databricks_contributor_dataplane_group.name
    permission_level = "CAN_MANAGE"
  }
  dynamic "access_control" {
    for_each = local.readers
    content {
      group_name       = access_control.key
      permission_level = "CAN_MONITOR"
    }
  }

  depends_on = [module.dbw]
}

module "st_dbw_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=storage-account-dfs_9.0.1"

  name                       = "dbwbackup"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  account_replication_type   = "GRS"
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_rules                   = local.ip_restrictions_as_string

  role_assignments = [
    {
      principal_id         = data.azurerm_key_vault_secret.shared_access_connector_principal_id.value
      role_definition_name = "Storage Blob Data Contributor"
    },
  ]
  audit_storage_account = var.enable_audit_logs ? {
    id = data.azurerm_key_vault_secret.st_audit_shres_id.value
  } : null
}

resource "azurerm_storage_container" "backup_results_internal" {
  name                  = azurerm_storage_container.results_internal.name
  storage_account_name  = module.st_dbw_backup.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "backup_basis_data_internal" {
  name                  = azurerm_storage_container.basis_data_internal.name
  storage_account_name  = module.st_dbw_backup.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "backup_internal" {
  name                  = azurerm_storage_container.internal.name
  storage_account_name  = module.st_dbw_backup.name
  container_access_type = "private"
}

