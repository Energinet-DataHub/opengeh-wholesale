resource "databricks_catalog_workspace_binding" "shared" {
  provider = databricks.dbw

  securable_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  workspace_id   = module.dbw.workspace_id

  depends_on = [module.dbw]
}

resource "databricks_external_location" "migrations_bronze_storage" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.bronze.name}_${module.st_migrations.name}"
  url             = "abfss://${azurerm_storage_container.bronze.name}@${module.st_migrations.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, databricks_catalog_workspace_binding.shared, module.st_migrations]
}

resource "databricks_schema" "migrations_bronze" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "migrations_bronze"
  comment      = "Migrations Bronze Schema"
  storage_root = databricks_external_location.migrations_bronze_storage.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}

resource "databricks_external_location" "migrations_silver_storage" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.silver.name}_${module.st_migrations.name}"
  url             = "abfss://${azurerm_storage_container.silver.name}@${module.st_migrations.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, databricks_catalog_workspace_binding.shared, module.st_migrations]
}

resource "databricks_schema" "migrations_silver" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "migrations_silver"
  comment      = "Migrations Silver Schema"
  storage_root = databricks_external_location.migrations_silver_storage.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}

resource "databricks_external_location" "migrations_gold_storage" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.gold.name}_${module.st_migrations.name}"
  url             = "abfss://${azurerm_storage_container.gold.name}@${module.st_migrations.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, databricks_catalog_workspace_binding.shared, module.st_migrations]
}

resource "databricks_schema" "migrations_gold" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "migrations_gold"
  comment      = "Migrations Gold Schema"
  storage_root = databricks_external_location.migrations_gold_storage.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}

resource "databricks_external_location" "eloverblik_storage" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.eloverblik.name}_${module.st_migrations.name}"
  url             = "abfss://${azurerm_storage_container.eloverblik.name}@${module.st_migrations.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, databricks_catalog_workspace_binding.shared, module.st_migrations]
}

resource "databricks_schema" "eloverblik" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "eloverblik"
  comment      = "Eloverblik Schema"
  storage_root = databricks_external_location.eloverblik_storage.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}

resource "databricks_external_location" "migrations_storage" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.schema_migration.name}_${module.st_migrations.name}"
  url             = "abfss://${azurerm_storage_container.schema_migration.name}@${module.st_migrations.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, databricks_catalog_workspace_binding.shared, module.st_migrations]
}

resource "databricks_schema" "migrations" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "migrations"
  comment      = "Migrations Schema"
  storage_root = databricks_external_location.migrations_storage.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}

data "azurerm_key_vault_secret" "unity_storage_credential_id" {
  name         = "unity-storage-credential-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "shared_unity_catalog_name" {
  name         = "shared-unity-catalog-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
