data "azurerm_key_vault_secret" "unity_storage_credential_id" {
  name         = "unity-storage-credential-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "shared_unity_catalog_name" {
  name         = "shared-unity-catalog-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

resource "databricks_external_location" "measurements_internal_storage" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.internal.name}_${module.st_measurements.name}"
  url             = "abfss://${azurerm_storage_container.internal.name}@${module.st_measurements.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, module.st_measurements]
}

resource "databricks_schema" "measurements_internal" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "measurements_internal"
  comment      = "Measurements Internal Schema"
  storage_root = databricks_external_location.measurements_internal_storage.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token]
}

resource "databricks_external_location" "measurements_bronze_storage" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.bronze.name}_${module.st_measurements.name}"
  url             = "abfss://${azurerm_storage_container.bronze.name}@${module.st_measurements.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, module.st_measurements]
}

resource "databricks_schema" "measurements_bronze" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "measurements_bronze"
  comment      = "Measurements Bronze Schema"
  storage_root = databricks_external_location.measurements_bronze_storage.url

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token]
}

resource "databricks_schema" "measurements_silver" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "measurements_silver"
  comment      = "Measurements Silver Schema"
  storage_root = databricks_external_location.measurements_silver_storage.url
}

resource "databricks_external_location" "measurements_silver_storage" {
  provider = databricks.dbw
  name     = "${azurerm_storage_container.silver.name}_${module.st_measurements.name}"
  url      = "abfss://${azurerm_storage_container.silver.name}@${module.st_measurements.name}.dfs.core.windows.net/"

  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, module.st_measurements]
}

resource "databricks_schema" "measurements_gold" {
  provider     = databricks.dbw
  catalog_name = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  name         = "measurements_gold"
  comment      = "Measurements Gold Schema"
  storage_root = databricks_external_location.measurements_gold_storage.url
}

resource "databricks_external_location" "measurements_gold_storage" {
  provider        = databricks.dbw
  name            = "${azurerm_storage_container.gold.name}_${module.st_measurements.name}"
  url             = "abfss://${azurerm_storage_container.gold.name}@${module.st_measurements.name}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"

  depends_on = [module.dbw, module.kvs_databricks_dbw_workspace_token]
}

#
# Shared Electricity Market storage account
#

# Give the access connector the necessary role on the storage account
resource "azurerm_role_assignment" "st_electricity_market_contributor" {
  scope                = data.azurerm_key_vault_secret.st_electricity_market_id.value
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azurerm_key_vault_secret.shared_access_connector_principal_id.value
}

resource "databricks_external_location" "shared_electricity_market_capacity_settlement_container" {
  provider        = databricks.dbw
  name            = "measurements_${data.azurerm_key_vault_secret.st_electricity_market_name.value}_${data.azurerm_key_vault_secret.st_electricity_market_capacity_settlement_container_name.value}"
  url             = "abfss://${data.azurerm_key_vault_secret.st_electricity_market_capacity_settlement_container_name.value}@${data.azurerm_key_vault_secret.st_electricity_market_name.value}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, azurerm_role_assignment.st_electricity_market_contributor]
}

resource "databricks_external_location" "shared_electricity_market_electrical_heating_container" {
  provider        = databricks.dbw
  name            = "measurements_${data.azurerm_key_vault_secret.st_electricity_market_name.value}_${data.azurerm_key_vault_secret.st_electricity_market_electrical_heating_container_name.value}"
  url             = "abfss://${data.azurerm_key_vault_secret.st_electricity_market_electrical_heating_container_name.value}@${data.azurerm_key_vault_secret.st_electricity_market_name.value}.dfs.core.windows.net/"
  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, azurerm_role_assignment.st_electricity_market_contributor]
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

resource "databricks_external_location" "measurements_calculated_storage" {
  provider = databricks.dbw
  name     = "${azurerm_storage_container.measurements_calculated.name}_${module.st_measurements.name}"
  url      = "abfss://${azurerm_storage_container.measurements_calculated.name}@${module.st_measurements.name}.dfs.core.windows.net/"

  credential_name = data.azurerm_key_vault_secret.unity_storage_credential_id.value
  comment         = "Managed by TF"
  depends_on      = [module.dbw, module.st_measurements]
}

#
# Volume Mounts
#
resource "databricks_volume" "shared_electricity_market_capacity_settlement_container" {
  provider         = databricks.dbw
  name             = "shared_electricity_market_capacity_settlement_container"
  catalog_name     = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  schema_name      = databricks_schema.measurements_internal.name
  volume_type      = "EXTERNAL"
  storage_location = databricks_external_location.shared_electricity_market_capacity_settlement_container.url
  comment          = "Managed by TF"
}
resource "databricks_volume" "shared_electricity_market_electrical_heating_container" {
  provider         = databricks.dbw
  name             = "shared_electricity_market_electrical_heating_container"
  catalog_name     = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  schema_name      = databricks_schema.measurements_internal.name
  volume_type      = "EXTERNAL"
  storage_location = databricks_external_location.shared_electricity_market_electrical_heating_container.url
  comment          = "Managed by TF"
}
resource "databricks_volume" "measurements_calculated_internal_storage" {
  provider         = databricks.dbw
  name             = "measurements_calculated_internal_storage"
  catalog_name     = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  schema_name      = databricks_schema.measurements_internal.name
  volume_type      = "EXTERNAL"
  storage_location = databricks_external_location.measurements_calculated_internal_storage.url
  comment          = "Managed by TF"
}
resource "databricks_volume" "measurements_calculated_storage" {
  provider         = databricks.dbw
  name             = "measurements_calculated_storage"
  catalog_name     = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
  schema_name      = databricks_schema.measurements_internal.name
  volume_type      = "EXTERNAL"
  storage_location = databricks_external_location.measurements_calculated_storage.url
  comment          = "Managed by TF"
}
resource "databricks_grants" "shared_electricity_market_capacity_settlement_container" {
  provider = databricks.dbw
  volume   = databricks_volume.shared_electricity_market_capacity_settlement_container.id
  grant {
    principal  = var.databricks_contributor_dataplane_group.name
    privileges = ["READ_VOLUME"]
  }
  grant {
    principal  = var.databricks_readers_group.name
    privileges = ["READ_VOLUME"]
  }
}
resource "databricks_grants" "shared_electricity_market_electrical_heating_container" {
  provider = databricks.dbw
  volume   = databricks_volume.shared_electricity_market_electrical_heating_container.id
  grant {
    principal  = var.databricks_contributor_dataplane_group.name
    privileges = ["READ_VOLUME"]
  }
  grant {
    principal  = var.databricks_readers_group.name
    privileges = ["READ_VOLUME"]
  }
}
resource "databricks_grants" "measurements_calculated_internal_storage" {
  provider = databricks.dbw
  volume   = databricks_volume.measurements_calculated_internal_storage.id
  grant {
    principal  = var.databricks_contributor_dataplane_group.name
    privileges = ["READ_VOLUME"]
  }
  grant {
    principal  = var.databricks_readers_group.name
    privileges = ["READ_VOLUME"]
  }
}
resource "databricks_grants" "measurements_calculated_storage" {
  provider = databricks.dbw
  volume   = databricks_volume.measurements_calculated_storage.id
  grant {
    principal  = var.databricks_contributor_dataplane_group.name
    privileges = ["READ_VOLUME"]
  }
  grant {
    principal  = var.databricks_readers_group.name
    privileges = ["READ_VOLUME"]
  }
}
