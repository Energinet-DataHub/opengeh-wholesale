data "azurerm_key_vault_secret" "unity_storage_credential_id" {
  name         = "unity-storage-credential-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "shared_unity_catalog_name" {
  name         = "shared-unity-catalog-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

resource "databricks_catalog_workspace_binding" "shared" {
  provider = databricks.dbw

  securable_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  workspace_id   = module.dbw.workspace_id

  depends_on = [module.dbw]
}

resource "databricks_external_location" "shared_delta_lake" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.storage_container.name}_${data.azurerm_key_vault_secret.st_data_lake_name.value}"
  url             = "abfss://${azurerm_storage_container.storage_container.name}@${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, databricks_catalog_workspace_binding.shared, data.azurerm_key_vault_secret.st_data_lake_name]
}

resource "databricks_schema" "shared_delta_lake_wholesale" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "wholesale"
  comment      = "Wholesale Schema"
  storage_root = databricks_external_location.shared_delta_lake.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}

resource "databricks_external_location" "internal" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.internal.name}_${module.st_data_wholesale.name}"
  url             = "abfss://${azurerm_storage_container.internal.name}@${module.st_data_wholesale.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, databricks_catalog_workspace_binding.shared, data.azurerm_key_vault_secret.st_data_lake_name]
}

resource "databricks_schema" "internal" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "wholesale_internal"
  comment      = "wholesale_internal Schema"
  storage_root = databricks_external_location.internal.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}

resource "databricks_external_location" "results_internal" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.results_internal.name}_${module.st_data_wholesale.name}"
  url             = "abfss://${azurerm_storage_container.results_internal.name}@${module.st_data_wholesale.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, databricks_catalog_workspace_binding.shared, data.azurerm_key_vault_secret.st_data_lake_name]
}

resource "databricks_schema" "results_internal" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "wholesale_results_internal"
  comment      = "wholesale_results_internal Schema"
  storage_root = databricks_external_location.results_internal.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}

resource "databricks_external_location" "basis_data_internal" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.basis_data_internal.name}_${module.st_data_wholesale.name}"
  url             = "abfss://${azurerm_storage_container.basis_data_internal.name}@${module.st_data_wholesale.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, databricks_catalog_workspace_binding.shared, data.azurerm_key_vault_secret.st_data_lake_name]
}

resource "databricks_schema" "basis_data_internal" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "wholesale_basis_data_internal"
  comment      = "wholesale_basis_data_internal Schema"
  storage_root = databricks_external_location.basis_data_internal.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}

resource "databricks_external_location" "settlement_reports" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.settlement_reports.name}_${module.st_data_wholesale.name}"
  url             = "abfss://${azurerm_storage_container.settlement_reports.name}@${module.st_data_wholesale.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, databricks_catalog_workspace_binding.shared, data.azurerm_key_vault_secret.st_data_lake_name]
}

resource "databricks_schema" "settlement_reports" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "wholesale_settlement_reports"
  comment      = "wholesale_settlement_reports Schema"
  storage_root = databricks_external_location.settlement_reports.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}
