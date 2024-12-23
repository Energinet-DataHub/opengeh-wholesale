data "azurerm_key_vault_secret" "unity_storage_credential_id" {
  name         = "unity-storage-credential-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "shared_unity_catalog_name" {
  name         = "shared-unity-catalog-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

resource "databricks_external_location" "internal" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.internal.name}_${module.st_data_wholesale.name}"
  url             = "abfss://${azurerm_storage_container.internal.name}@${module.st_data_wholesale.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, data.azurerm_key_vault_secret.st_data_lake_name]
}

resource "databricks_schema" "internal" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "wholesale_internal"
  comment      = "wholesale_internal Schema"
  storage_root = databricks_external_location.internal.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token]
}

resource "databricks_external_location" "results_internal" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.results_internal.name}_${module.st_data_wholesale.name}"
  url             = "abfss://${azurerm_storage_container.results_internal.name}@${module.st_data_wholesale.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, data.azurerm_key_vault_secret.st_data_lake_name]
}

resource "databricks_schema" "results_internal" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "wholesale_results_internal"
  comment      = "wholesale_results_internal Schema"
  storage_root = databricks_external_location.results_internal.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token]
}

resource "databricks_external_location" "results" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.results.name}_${module.st_data_wholesale.name}"
  url             = "abfss://${azurerm_storage_container.results.name}@${module.st_data_wholesale.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, data.azurerm_key_vault_secret.st_data_lake_name]
}

resource "databricks_schema" "results" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "wholesale_results"
  comment      = "wholesale_results Schema"
  storage_root = databricks_external_location.results.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token]
}

resource "databricks_external_location" "basis_data_internal" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.basis_data_internal.name}_${module.st_data_wholesale.name}"
  url             = "abfss://${azurerm_storage_container.basis_data_internal.name}@${module.st_data_wholesale.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, data.azurerm_key_vault_secret.st_data_lake_name]
}

resource "databricks_schema" "basis_data_internal" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "wholesale_basis_data_internal"
  comment      = "wholesale_basis_data_internal Schema"
  storage_root = databricks_external_location.basis_data_internal.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token]
}

resource "databricks_external_location" "basis_data" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.basis_data.name}_${module.st_data_wholesale.name}"
  url             = "abfss://${azurerm_storage_container.basis_data.name}@${module.st_data_wholesale.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, data.azurerm_key_vault_secret.st_data_lake_name]
}

resource "databricks_schema" "basis_data" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "wholesale_basis_data"
  comment      = "wholesale_basis_data Schema"
  storage_root = databricks_external_location.basis_data.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token]
}

resource "databricks_external_location" "settlement_reports" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.settlement_reports.name}_${module.st_data_wholesale.name}"
  url             = "abfss://${azurerm_storage_container.settlement_reports.name}@${module.st_data_wholesale.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, data.azurerm_key_vault_secret.st_data_lake_name]
  force_destroy = true
}

resource "databricks_schema" "settlement_reports" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "wholesale_settlement_reports"
  comment      = "wholesale_settlement_reports Schema"
  storage_root = databricks_external_location.settlement_reports.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token]
}

resource "databricks_external_location" "sap" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.sap.name}_${module.st_data_wholesale.name}"
  url             = "abfss://${azurerm_storage_container.sap.name}@${module.st_data_wholesale.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, data.azurerm_key_vault_secret.st_data_lake_name]
}

resource "databricks_schema" "sap" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "wholesale_sap"
  comment      = "wholesale_sap Schema"
  storage_root = databricks_external_location.sap.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token]
}

# Settlement Report storage account is created in shared together with external location
resource "databricks_schema" "settlement_report_output" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "wholesale_settlement_report_output"
  comment      = "wholesale_settlement_report_output Schema"
  storage_root = data.azurerm_key_vault_secret.settlement_report_external_location_url.value

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token]
}

resource "databricks_volume" "settlement_reports" {
  provider         = databricks.dbw
  name             = "settlement_reports"
  catalog_name     = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  schema_name      = databricks_schema.settlement_report_output.name
  volume_type      = "EXTERNAL"
  storage_location = "${data.azurerm_key_vault_secret.settlement_report_external_location_url.value}reports"
  comment          = "this volume is managed by terraform"
}
