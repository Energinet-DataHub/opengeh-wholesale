resource "databricks_job" "calculator_job" {
  name = "CalculatorJob"
  max_concurrent_runs = 100
  always_running = false

  task {
    task_key = "calculator_job_${uuid()}"
    max_retries = 0

    new_cluster {
      spark_version           = data.databricks_spark_version.latest_lts.id
      node_type_id            = "Standard_DS4_v2"
      autoscale {
        min_workers = 4
        max_workers = 8
      }
    }

    library {
      whl = "dbfs:/opengeh-wholesale/package-1.0-py3-none-any.whl"
    } 

    python_wheel_task {
      package_name = "package"
      # The entry point is defined in setup.py
      entry_point = "start_calculator"
      parameters  = [
          "--data-storage-account-name=${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}",
          "--data-storage-account-key=${data.azurerm_key_vault_secret.kvs_st_data_lake_primary_access_key.value}",
          "--time-zone=${local.TIME_ZONE}",
          "--log-level=information"
      ]
    }
  }

  email_notifications {
    no_alert_for_skipped_runs = true
  }
}
