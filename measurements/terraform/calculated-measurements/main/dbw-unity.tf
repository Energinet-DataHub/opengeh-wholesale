data "azurerm_key_vault_secret" "unity_storage_credential_id" {
  name         = "unity-storage-credential-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "shared_unity_catalog_name" {
  name         = "shared-unity-catalog-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

#
# Measurements Calculated storage account
#

resource "databricks_external_location" "measurements_calculated_internal_storage" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.measurements_calculated_internal.name}_${module.st_measurements.name}"
  url             = "abfss://${azurerm_storage_container.measurements_calculated_internal.name}@${module.st_measurements.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, module.st_measurements]
}

resource "databricks_schema" "measurements_calculated_internal" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = local.database_measurements_calculated_internal
  comment      = "Measurements Calculated Internal Schema"
  storage_root = databricks_external_location.measurements_calculated_internal_storage.url
  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token]
}

resource "databricks_external_location" "measurements_calculated_storage" {
  provider = databricks.dbw
  name     = "${azurerm_storage_container.measurements_calculated.name}_${module.st_measurements.name}"
  url      = "abfss://${azurerm_storage_container.measurements_calculated.name}@${module.st_measurements.name}.dfs.core.windows.net/"

  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, module.st_measurements]
}

resource "databricks_schema" "measurements_calculated" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = local.measurements_calculated
  comment      = "Measurements Calculated Schema"
  storage_root = databricks_external_location.measurements_calculated_storage.url
}