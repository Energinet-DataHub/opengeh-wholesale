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

# TEMPORARY: give developers access to copy data from hive_metastore to unity catalog
resource "databricks_grant" "dev_external_location" {
  provider          = databricks.dbw
  external_location = databricks_external_location.migrations_silver_storage.id
  principal         = var.migration_group_name
  privileges        = ["READ_FILES", "WRITE_FILES"]

  depends_on = [module.dbw, databricks_catalog_workspace_binding.shared]
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

# TEMPORARY: give developers access to copy data from hive_metastore to unity catalog
resource "databricks_grant" "dev_external_location_gold" {
  provider          = databricks.dbw
  external_location = databricks_external_location.migrations_gold_storage.id
  principal         = var.migration_group_name
  privileges        = ["READ_FILES", "WRITE_FILES"]

  depends_on = [module.dbw, databricks_catalog_workspace_binding.shared]
}

resource "databricks_schema" "migrations_gold" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "migrations_gold"
  comment      = "Migrations Gold Schema"
  storage_root = databricks_external_location.migrations_gold_storage.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}

resource "databricks_external_location" "migrations_eloverblik_storage" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.eloverblik.name}_${module.st_migrations.name}"
  url             = "abfss://${azurerm_storage_container.eloverblik.name}@${module.st_migrations.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, databricks_catalog_workspace_binding.shared, module.st_migrations]
}

# TEMPORARY: give developers access to copy data from hive_metastore to unity catalog
resource "databricks_grant" "dev_external_location_eloverblik" {
  provider          = databricks.dbw
  external_location = databricks_external_location.migrations_eloverblik_storage.id
  principal         = var.migration_group_name
  privileges        = ["READ_FILES", "WRITE_FILES"]

  depends_on = [module.dbw, databricks_catalog_workspace_binding.shared]
}

resource "databricks_schema" "migrations_eloverblik" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "migrations_eloverblik"
  comment      = "Migrations Eloverblik Schema"
  storage_root = databricks_external_location.migrations_eloverblik_storage.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}

resource "databricks_external_location" "migrations_internal_storage" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.internal.name}_${module.st_migrations.name}"
  url             = "abfss://${azurerm_storage_container.internal.name}@${module.st_migrations.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, databricks_catalog_workspace_binding.shared, module.st_migrations]
}

resource "databricks_schema" "migrations_internal" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "migrations_internal"
  comment      = "Migrations Internal Schema"
  storage_root = databricks_external_location.migrations_internal_storage.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}

resource "databricks_external_location" "migrations_wholesale_storage" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.wholesale.name}_${module.st_migrations.name}"
  url             = "abfss://${azurerm_storage_container.wholesale.name}@${module.st_migrations.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, databricks_catalog_workspace_binding.shared, module.st_migrations]
}

# TEMPORARY: give developers access to copy data from hive_metastore to unity catalog
resource "databricks_grant" "dev_external_location_wholesale" {
  provider          = databricks.dbw
  external_location = databricks_external_location.migrations_wholesale_storage.id
  principal         = var.migration_group_name
  privileges        = ["READ_FILES", "WRITE_FILES"]

  depends_on = [module.dbw, databricks_catalog_workspace_binding.shared]
}

resource "databricks_schema" "migrations_wholesale" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "migrations_wholesale"
  comment      = "Migrations Wholesale Schema"
  storage_root = databricks_external_location.migrations_wholesale_storage.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}

resource "databricks_external_location" "shared_wholesale_input" {
  provider        = databricks.dbw
  name            = "wholesaleinput_${data.azurerm_key_vault_secret.st_data_lake_name.value}"
  url             = "abfss://wholesaleinput@${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, databricks_catalog_workspace_binding.shared, module.st_migrations]
}

# TEMPORARY: give developers access to copy data from hive_metastore to unity catalog
resource "databricks_grant" "dev_external_location_shared_wholesale_input" {
  provider          = databricks.dbw
  external_location = databricks_external_location.shared_wholesale_input.id
  principal         = var.migration_group_name
  privileges        = ["READ_FILES", "WRITE_FILES"]

  depends_on = [module.dbw, databricks_catalog_workspace_binding.shared]
}

resource "databricks_schema" "shared_wholesale_input" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "shared_wholesale_input"
  comment      = "Shared Wholesale Schema"
  storage_root = databricks_external_location.shared_wholesale_input.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token, databricks_catalog_workspace_binding.shared]
}

data "azurerm_key_vault_secret" "unity_storage_credential_id" {
  name         = "unity-storage-credential-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
