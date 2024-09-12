resource "databricks_job" "settlement_report_job" {
  provider            = databricks.dbw
  name                = "SettlementReportJob"
  max_concurrent_runs = 125

  task {
    task_key    = "settlement_report_job_${uuid()}"
    max_retries = 1

    new_cluster {
      spark_version = local.spark_version
      node_type_id  = "Standard_D8as_v4"
      runtime_engine = "PHOTON"
      autoscale {
        min_workers = 1
        max_workers = 10
      }
      spark_env_vars = {
        "DATA_STORAGE_ACCOUNT_NAME"         = data.azurerm_key_vault_secret.st_data_lake_name.value
        "TIME_ZONE"                         = local.TIME_ZONE
        "CATALOG_NAME"                      = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
      }
    }

    library {
      whl = "/Workspace/Shared/PythonWheels/settlement_report/opengeh_settlement_report-1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "opengeh_settlement_report"
      # The entry point is defined in setup.py
      entry_point = "create_settlement_report"
    }
  }

  email_notifications {
    no_alert_for_skipped_runs = true
  }
}
