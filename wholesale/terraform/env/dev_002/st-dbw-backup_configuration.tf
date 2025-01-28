module "results_internal_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-storage-backup?ref=databricks-storage-backup_6.0.2"
  providers = { # The module requires a databricks provider, as it uses databricks resources
    databricks = databricks.dbw
  }

  backup_storage_account_name   = module.st_dbw_backup.name
  backup_container_name         = azurerm_storage_container.backup_results_internal.name
  unity_storage_credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  shared_unity_catalog_name     = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  backup_schema_name            = "${databricks_schema.results_internal.name}_backup"
  backup_schema_comment         = databricks_schema.results_internal.comment
  tables                        = local.results_internal_schema
  source_schema_name            = databricks_schema.results_internal.name
  backup_sql_endpoint_id        = databricks_sql_endpoint.backup_warehouse[local.warehouse_key].id
  access_control                = local.backup_access_control
  backup_email_on_failure       = var.alert_email_address != null ? [var.alert_email_address] : []

  depends_on = [module.dbw]
}

module "internal_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-storage-backup?ref=databricks-storage-backup_6.0.2"
  providers = { # The module requires a databricks provider, as it uses databricks resources
    databricks = databricks.dbw
  }

  backup_storage_account_name   = module.st_dbw_backup.name
  backup_container_name         = azurerm_storage_container.backup_internal.name
  unity_storage_credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  shared_unity_catalog_name     = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  backup_schema_name            = "${databricks_schema.internal.name}_backup"
  backup_schema_comment         = databricks_schema.internal.comment
  tables                        = local.internal_schema
  source_schema_name            = databricks_schema.internal.name
  backup_sql_endpoint_id        = databricks_sql_endpoint.backup_warehouse[local.warehouse_key].id
  access_control                = local.backup_access_control
  backup_email_on_failure       = var.alert_email_address != null ? [var.alert_email_address] : []

  depends_on = [module.dbw]
}
