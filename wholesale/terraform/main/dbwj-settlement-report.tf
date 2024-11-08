resource "databricks_job" "settlement_report_job_balance_fixing" {
  provider            = databricks.dbw
  name                = "SettlementReportBalanceFixing"
  max_concurrent_runs = 40

  job_cluster {
    job_cluster_key = "hourly_time_series_cluster"

    new_cluster {
      spark_version  = local.spark_version
      node_type_id   = "Standard_D8as_v4"
      runtime_engine = "PHOTON"
      autoscale {
        min_workers = 1
        max_workers = 10
      }
      spark_env_vars = {
        "DATA_STORAGE_ACCOUNT_NAME"             = data.azurerm_key_vault_secret.st_data_lake_name.value
        "TIME_ZONE"                             = local.TIME_ZONE
        "CATALOG_NAME"                          = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
        "APPLICATIONINSIGHTS_CONNECTION_STRING" = data.azurerm_key_vault_secret.appi_shared_connection_string.value
      }
    }
  }

  job_cluster {
    job_cluster_key = "settlement_report_cluster"

    new_cluster {
      spark_version  = local.spark_version
      node_type_id   = "Standard_D8as_v4"
      runtime_engine = "PHOTON"
      autoscale {
        min_workers = 1
        max_workers = 10
      }
      spark_env_vars = {
        "DATA_STORAGE_ACCOUNT_NAME"             = data.azurerm_key_vault_secret.st_data_lake_name.value
        "TIME_ZONE"                             = local.TIME_ZONE
        "CATALOG_NAME"                          = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
        "APPLICATIONINSIGHTS_CONNECTION_STRING" = data.azurerm_key_vault_secret.appi_shared_connection_string.value
      }
    }
  }

  task {
    task_key        = "hourly_time_series"
    max_retries     = 1
    job_cluster_key = "hourly_time_series_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/settlement_report/opengeh_settlement_report-1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "opengeh_settlement_report"
      # The entry point is defined in setup.py
      entry_point = "create_hourly_time_series"
    }
  }

  task {
    task_key        = "quarterly_time_series"
    max_retries     = 1
    job_cluster_key = "settlement_report_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/settlement_report/opengeh_settlement_report-1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "opengeh_settlement_report"
      # The entry point is defined in setup.py
      entry_point = "create_quarterly_time_series"
    }
  }

  task {
    task_key        = "energy_results"
    max_retries     = 1
    job_cluster_key = "settlement_report_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/settlement_report/opengeh_settlement_report-1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "opengeh_settlement_report"
      # The entry point is defined in setup.py
      entry_point = "create_energy_results"
    }
  }

  task {
    task_key        = "zip"
    max_retries     = 1
    job_cluster_key = "settlement_report_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/settlement_report/opengeh_settlement_report-1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "opengeh_settlement_report"
      # The entry point is defined in setup.py
      entry_point = "create_zip"
    }
    depends_on {
      task_key = "hourly_time_series"
    }
    depends_on {
      task_key = "quarterly_time_series"
    }
    depends_on {
      task_key = "energy_results"
    }
  }

  email_notifications {
    no_alert_for_skipped_runs = true
  }

  queue {
    enabled = true
  }
}

resource "databricks_job" "settlement_report_job_wholesale" {
  provider            = databricks.dbw
  name                = "SettlementReportWholesaleCalculations"
  max_concurrent_runs = 40

  job_cluster {
    job_cluster_key = "hourly_time_series_cluster"

    new_cluster {
      spark_version  = local.spark_version
      node_type_id   = "Standard_D8as_v4"
      runtime_engine = "PHOTON"
      autoscale {
        min_workers = 1
        max_workers = 10
      }
      spark_env_vars = {
        "DATA_STORAGE_ACCOUNT_NAME"             = data.azurerm_key_vault_secret.st_data_lake_name.value
        "TIME_ZONE"                             = local.TIME_ZONE
        "CATALOG_NAME"                          = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
        "APPLICATIONINSIGHTS_CONNECTION_STRING" = data.azurerm_key_vault_secret.appi_shared_connection_string.value
      }
    }
  }

  job_cluster {
    job_cluster_key = "settlement_report_cluster"

    new_cluster {
      spark_version  = local.spark_version
      node_type_id   = "Standard_D8as_v4"
      runtime_engine = "PHOTON"
      autoscale {
        min_workers = 1
        max_workers = 10
      }

      spark_env_vars = {
        "DATA_STORAGE_ACCOUNT_NAME"             = data.azurerm_key_vault_secret.st_data_lake_name.value
        "TIME_ZONE"                             = local.TIME_ZONE
        "CATALOG_NAME"                          = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
        "APPLICATIONINSIGHTS_CONNECTION_STRING" = data.azurerm_key_vault_secret.appi_shared_connection_string.value
      }
    }
  }

  task {
    task_key        = "hourly_time_series"
    max_retries     = 1
    job_cluster_key = "hourly_time_series_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/settlement_report/opengeh_settlement_report-1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "opengeh_settlement_report"
      # The entry point is defined in setup.py
      entry_point = "create_hourly_time_series"
    }
  }

  task {
    task_key        = "quarterly_time_series"
    max_retries     = 1
    job_cluster_key = "settlement_report_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/settlement_report/opengeh_settlement_report-1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "opengeh_settlement_report"
      # The entry point is defined in setup.py
      entry_point = "create_quarterly_time_series"
    }
  }

  task {
    task_key        = "metering_point_periods"
    max_retries     = 1
    job_cluster_key = "settlement_report_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/settlement_report/opengeh_settlement_report-1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "opengeh_settlement_report"
      # The entry point is defined in setup.py
      entry_point = "create_metering_point_periods"
    }
  }

  task {
    task_key        = "charge_links"
    max_retries     = 1
    job_cluster_key = "settlement_report_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/settlement_report/opengeh_settlement_report-1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "opengeh_settlement_report"
      # The entry point is defined in setup.py
      entry_point = "create_charge_links"
    }
  }

  task {
    task_key        = "energy_results"
    max_retries     = 1
    job_cluster_key = "settlement_report_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/settlement_report/opengeh_settlement_report-1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "opengeh_settlement_report"
      # The entry point is defined in setup.py
      entry_point = "create_energy_results"
    }
  }

  task {
    task_key        = "wholesale_results"
    max_retries     = 1
    job_cluster_key = "settlement_report_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/settlement_report/opengeh_settlement_report-1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "opengeh_settlement_report"
      # The entry point is defined in setup.py
      entry_point = "create_wholesale_results"
    }
  }

  task {
    task_key    = "monthly_amounts"
    max_retries = 1
	job_cluster_key = "settlement_report_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/settlement_report/opengeh_settlement_report-1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "opengeh_settlement_report"
      # The entry point is defined in setup.py
      entry_point = "create_monthly_amounts"
    }
  }

  task {
    task_key        = "zip"
    max_retries     = 1
    job_cluster_key = "settlement_report_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/settlement_report/opengeh_settlement_report-1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "opengeh_settlement_report"
      # The entry point is defined in setup.py
      entry_point = "create_zip"
    }
    depends_on {
      task_key = "charge_links"
    }
    depends_on {
      task_key = "hourly_time_series"
    }
    depends_on {
      task_key = "quarterly_time_series"
    }
    depends_on {
      task_key = "metering_point_periods"
    }
    depends_on {
      task_key = "energy_results"
    }
    depends_on {
      task_key = "wholesale_results"
    }
    depends_on {
      task_key = "monthly_amounts"
    }
  }

  email_notifications {
    no_alert_for_skipped_runs = true
  }

  queue {
    enabled = true
  }
}
