resource "databricks_job" "migrations_job" {
  provider            = databricks.dbw
  name                = "MigrationsJob"
  max_concurrent_runs = 1

  task {
    task_key    = "migrations_job_${uuid()}"
    max_retries = 1

    new_cluster {
      spark_version = local.spark_version
      node_type_id  = "Standard_D64as_v4"
      autoscale {
        min_workers = 1
        max_workers = 10
      }
      spark_conf = {
        "fs.azure.account.oauth2.client.endpoint.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "https://login.microsoftonline.com/${var.tenant_id}/oauth2/token"
        "fs.azure.account.auth.type.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "OAuth"
        "fs.azure.account.oauth.provider.type.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
        "fs.azure.account.oauth2.client.id.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference
        "fs.azure.account.oauth2.client.secret.${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference
        "spark.databricks.sql.initial.catalog.name" : data.azurerm_key_vault_secret.shared_unity_catalog_name.value
      }
      spark_env_vars = {
        "TENANT_ID"                       = var.tenant_id,
        "SPN_APP_ID"                      = databricks_secret.spn_app_id.config_reference
        "SPN_APP_SECRET"                  = databricks_secret.spn_app_secret.config_reference
        "DATA_STORAGE_ACCOUNT_NAME"       = data.azurerm_key_vault_secret.st_data_lake_name.value
        "TIME_ZONE"                       = local.TIME_ZONE
        "CATALOG_NAME"                    = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
        "CALCULATION_INPUT_FOLDER_NAME"   = var.calculation_input_folder
        "CALCULATION_INPUT_DATABASE_NAME" = var.calculation_input_database
      }
    }

    library {
      whl = "/Workspace/Shared/PythonWheels/calculation_engine/package-1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "package"
      # The entry point is defined in setup.py
      entry_point = "migrate_data_lake"
    }
  }

  email_notifications {
    no_alert_for_skipped_runs = true
  }
}
