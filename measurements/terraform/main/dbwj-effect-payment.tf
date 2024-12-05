resource "databricks_job" "effect_payment" {
  provider            = databricks.dbw
  name                = "EffectPayment"
  max_concurrent_runs = 1

  job_cluster {
    job_cluster_key = "effect_payment_cluster"

    new_cluster {
      spark_version  = local.spark_version
      node_type_id   = "Standard_D8as_v4"
      runtime_engine = "PHOTON"
      autoscale {
        min_workers = 1
        max_workers = 10
      }
      spark_env_vars = {
        "TIME_ZONE"                             = local.TIME_ZONE
        "CATALOG_NAME"                          = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
        "APPLICATIONINSIGHTS_CONNECTION_STRING" = data.azurerm_key_vault_secret.appi_shared_connection_string.value
      }
    }
  }

  task {
    task_key        = "effect_payment_task"
    max_retries     = 1
    job_cluster_key = "effect_payment_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/effect_payment/opengeh_effect_payment-1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "opengeh_effect_payment"
      # The entry point is defined in setup.py
      entry_point = "execute"
    }
  }

  email_notifications {
    no_alert_for_skipped_runs = false
  }

  queue {
    enabled = false
  }
}
