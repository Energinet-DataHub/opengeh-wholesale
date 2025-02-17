module "results_internal_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-storage-backup?ref=databricks-storage-backup_8.0.0"
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
  backup_cluster_id             = databricks_cluster.backup_cluster[local.backup_key].id
  access_control                = local.backup_access_control
  backup_email_on_failure       = var.alert_email_address != null ? [var.alert_email_address] : []

  depends_on = [module.dbw]
}

module "basis_data_internal_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-storage-backup?ref=databricks-storage-backup_8.0.0"
  providers = { # The module requires a databricks provider, as it uses databricks resources
    databricks = databricks.dbw
  }

  backup_storage_account_name   = module.st_dbw_backup.name
  backup_container_name         = azurerm_storage_container.backup_basis_data_internal.name
  unity_storage_credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  shared_unity_catalog_name     = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  backup_schema_name            = "${databricks_schema.basis_data_internal.name}_backup"
  backup_schema_comment         = databricks_schema.basis_data_internal.comment
  tables                        = local.basis_data_internal_schema
  source_schema_name            = databricks_schema.basis_data_internal.name
  backup_cluster_id             = databricks_cluster.backup_cluster[local.backup_key].id
  access_control                = local.backup_access_control
  backup_email_on_failure       = var.alert_email_address != null ? [var.alert_email_address] : []

  depends_on = [module.dbw]
}

module "internal_backup" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-storage-backup?ref=databricks-storage-backup_8.0.0"
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
  backup_cluster_id             = databricks_cluster.backup_cluster[local.backup_key].id
  access_control                = local.backup_access_control
  backup_email_on_failure       = var.alert_email_address != null ? [var.alert_email_address] : []

  depends_on = [module.dbw]
}
