module "internal_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-storage-backup?ref=databricks-storage-backup_9.0.0"
  providers = {
    databricks = databricks.dbw
  }

  backup_storage_account_name   = module.st_migrations_backup.name
  backup_container_name         = azurerm_storage_container.internal_backup.name
  unity_storage_credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  shared_unity_catalog_name     = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  backup_schema_name            = "${databricks_schema.migrations_internal.name}_backup"
  backup_schema_comment         = databricks_schema.migrations_internal.comment
  tables = {
    InternalExecutedMigrations = {
      table_name = "executed_migrations"
    }
  }
  source_schema_name                     = databricks_schema.migrations_internal.name
  access_control                         = local.backup_access_control
  backup_email_on_failure                = var.alert_email_address != null ? [var.alert_email_address] : []
  backup_schedule_quartz_cron_expression = "0 0 * ? * *"

  depends_on = [module.dbw]
}

module "bronze_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-storage-backup?ref=databricks-storage-backup_9.0.0"
  providers = {
    databricks = databricks.dbw
  }

  backup_storage_account_name   = module.st_migrations_backup.name
  backup_container_name         = azurerm_storage_container.bronze_backup.name
  unity_storage_credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  shared_unity_catalog_name     = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  backup_schema_name            = "${databricks_schema.migrations_bronze.name}_backup"
  backup_schema_comment         = databricks_schema.migrations_bronze.comment
  tables = {
    BronzeChargeLinks = {
      table_name = "charge_links"
    }
    BronzeCharges = {
      table_name = "charges"
    }
    BronzeMeteringPoints = {
      table_name = "metering_points"
    }
    BronzeTimeSeries = {
      table_name = "time_series"
    }
    BronzeMeteringPointsQuarantined = {
      table_name = "metering_points_quarantined"
    }
    BronzeTimeSeriesQuarantined = {
      table_name = "time_series_quarantined"
    }
  }
  source_schema_name                     = databricks_schema.migrations_bronze.name
  access_control                         = local.backup_access_control
  backup_email_on_failure                = var.alert_email_address != null ? [var.alert_email_address] : []
  backup_schedule_quartz_cron_expression = "0 0/15 * ? * *"

  depends_on = [module.dbw]
}

module "silver_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-storage-backup?ref=databricks-storage-backup_9.0.0"
  providers = {
    databricks = databricks.dbw
  }

  backup_storage_account_name   = module.st_migrations_backup.name
  backup_container_name         = azurerm_storage_container.silver_backup.name
  unity_storage_credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  shared_unity_catalog_name     = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  backup_schema_name            = "${databricks_schema.migrations_silver.name}_backup"
  backup_schema_comment         = databricks_schema.migrations_silver.comment
  tables = {
    SilverMeteringPointsConnReg = {
      table_name = "metering_points_connection_register"
    }
    SilverMeteringPointsDossInfo = {
      table_name = "metering_points_business_transaction_dossiers_info"
    }
    SilverMeteringPointsDossSteps = {
      table_name = "metering_points_business_transaction_dossiers_steps"
    }
    SilverTimeSeries = {
      table_name = "time_series"
    }
    SilverTimeSeriesMasterdata = {
      table_name = "time_series_masterdata"
    }
    SilverMeteringPointsConnRegQuarantined = {
      table_name = "metering_points_connection_register_quarantined"
    }
    SilverTimeSeriesQuarantined = {
      table_name = "time_series_quarantined"
    }
  }
  source_schema_name                     = databricks_schema.migrations_silver.name
  access_control                         = local.backup_access_control
  backup_email_on_failure                = var.alert_email_address != null ? [var.alert_email_address] : []
  backup_schedule_quartz_cron_expression = "0 0 0/4 ? * *"

  depends_on = [module.dbw]
}

module "gold_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-storage-backup?ref=databricks-storage-backup_9.0.0"
  providers = {
    databricks = databricks.dbw
  }

  backup_storage_account_name   = module.st_migrations_backup.name
  backup_container_name         = azurerm_storage_container.gold_backup.name
  unity_storage_credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  shared_unity_catalog_name     = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  backup_schema_name            = "${databricks_schema.migrations_gold.name}_backup"
  backup_schema_comment         = databricks_schema.migrations_gold.comment
  tables = {
    GoldMeteringPoints = {
      table_name = "metering_points"
    }
    GoldTimeSeries = {
      table_name = "time_series_points"
    }
  }
  source_schema_name                     = databricks_schema.migrations_gold.name
  access_control                         = local.backup_access_control
  backup_email_on_failure                = var.alert_email_address != null ? [var.alert_email_address] : []
  backup_schedule_quartz_cron_expression = "0 0 0/12 ? * *"

  depends_on = [module.dbw]
}

module "shared_wholesale_input_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-storage-backup?ref=databricks-storage-backup_9.0.0"
  providers = {
    databricks = databricks.dbw
  }

  backup_storage_account_name   = module.st_migrations_backup.name
  backup_container_name         = azurerm_storage_container.shared_wholesale_input_backup.name
  unity_storage_credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  shared_unity_catalog_name     = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  backup_schema_name            = "${databricks_schema.shared_wholesale_input.name}_backup"
  backup_schema_comment         = databricks_schema.shared_wholesale_input.comment
  tables = {
    SharedWholesaleMeteringPoints = {
      table_name = "metering_point_periods"
    }
    SharedWholesaleTimeSeries = {
      table_name = "time_series_points"
    }
  }
  source_schema_name                     = databricks_schema.shared_wholesale_input.name
  access_control                         = local.backup_access_control
  backup_email_on_failure                = var.alert_email_address != null ? [var.alert_email_address] : []
  backup_schedule_quartz_cron_expression = "0 0 0/12 ? * *"

  depends_on = [module.dbw]
}

module "eloverblik_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-storage-backup?ref=databricks-storage-backup_9.0.0"
  providers = {
    databricks = databricks.dbw
  }

  backup_storage_account_name   = module.st_migrations_backup.name
  backup_container_name         = azurerm_storage_container.eloverblik_backup.name
  unity_storage_credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  shared_unity_catalog_name     = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  backup_schema_name            = "${databricks_schema.migrations_eloverblik.name}_backup"
  backup_schema_comment         = databricks_schema.migrations_eloverblik.comment
  tables = {
    EloverblikTimeSeries = {
      table_name = "eloverblik_time_series_points"
    }
  }
  source_schema_name                     = databricks_schema.migrations_eloverblik.name
  access_control                         = local.backup_access_control
  backup_email_on_failure                = var.alert_email_address != null ? [var.alert_email_address] : []
  backup_schedule_quartz_cron_expression = "0 0 0/12 ? * *"

  depends_on = [module.dbw]
}
