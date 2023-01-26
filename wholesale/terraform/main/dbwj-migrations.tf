resource "databricks_job" "migrations_job" {
  name = "MigrationsJob"
  max_concurrent_runs = 1
  always_running = false

  task {
    task_key = "migrations_job_${uuid()}"
    max_retries = 1

    new_cluster {
      spark_version           = data.databricks_spark_version.latest_lts.id
      node_type_id            = "Standard_DS3_v2"
      autoscale {
        min_workers = 1
        max_workers = 4
      }
    }

    library {
      whl = "dbfs:/opengeh-wholesale/package-1.0-py3-none-any.whl"
    } 

    python_wheel_task {
      package_name = "package"
      # The entry point is defined in setup.py
      entry_point = "migrate_data_lake"
      parameters  = [
          "--data-storage-account-name=${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}",
          "--data-storage-account-key=${data.azurerm_key_vault_secret.kvs_st_data_lake_primary_access_key.value}",
          "--log-level=information"
      ]
    }
  }
  
  email_notifications {
    no_alert_for_skipped_runs = true
  }
}
