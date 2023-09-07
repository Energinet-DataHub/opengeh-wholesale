resource "databricks_job" "migrations_job" {
  name                = "MigrationsJob"
  max_concurrent_runs = 1
  always_running      = false

  task {
    task_key    = "migrations_job_${uuid()}"
    max_retries = 1

    new_cluster {
      spark_version = data.databricks_spark_version.latest_lts.id
      node_type_id  = "Standard_DS3_v2"
      autoscale {
        min_workers = 1
        max_workers = 4
      }
      spark_conf = {
        "fs.azure.account.oauth2.client.endpoint.${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net" : "https://login.microsoftonline.com/${var.tenant_id}/oauth2/token"
        "fs.azure.account.auth.type.${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net" : "OAuth"
        "fs.azure.account.oauth.provider.type.${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net" : "org.apache.hadoop.fs.azurebfs.oauth2.ClientCredsTokenProvider"
        "fs.azure.account.oauth2.client.id.${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net" : databricks_secret.spn_app_id.config_reference
        "fs.azure.account.oauth2.client.secret.${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net" : databricks_secret.spn_app_secret.config_reference
        "spark.databricks.delta.preview.enabled" : true
      }
      spark_env_vars = {
        "TENANT_ID"                 = var.tenant_id,
        "SPN_APP_ID"                = databricks_secret.spn_app_id.config_reference
        "SPN_APP_SECRET"            = databricks_secret.spn_app_secret.config_reference
        "DATA_STORAGE_ACCOUNT_NAME" = data.azurerm_key_vault_secret.st_shared_data_lake_name.value
        "TIME_ZONE"                 = local.TIME_ZONE
      }
    }

    library {
      whl = "dbfs:/opengeh-wholesale/package-1.0-py3-none-any.whl"
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
