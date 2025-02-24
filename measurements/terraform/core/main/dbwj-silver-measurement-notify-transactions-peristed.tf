resource "databricks_job" "silver_notify_transactions_persisted_stream" {
  provider            = databricks.dbw
  name                = "Silver Notify Transactions Persisted Stream"
  max_concurrent_runs = 1

  job_cluster {
    job_cluster_key = "silver_notify_transactions_persisted_stream_cluster"

    new_cluster {
      spark_version  = local.spark_version
      node_type_id   = "Standard_D8as_v4"
      runtime_engine = "PHOTON"
      autoscale {
        min_workers = 1
        max_workers = 10
      }
      spark_conf = {
        "spark.databricks.sql.initial.catalog.name" = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
      }

      spark_env_vars = {
        "spark.databricks.sql.initial.catalog.name" = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
        "CATALOG_NAME"                              = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
        "APPLICATIONINSIGHTS_CONNECTION_STRING"     = data.azurerm_key_vault_secret.appi_shared_connection_string.value
        "EVENT_HUB_NAMESPACE"                       = data.azurerm_key_vault_secret.evhns_measurements_name.value
        "EVENT_HUB_INSTANCE"                        = data.azurerm_key_vault_secret.evh_measurement_transactions_receipts_name.value
        "TENANT_ID"                                 = var.tenant_id,
        "SPN_APP_ID"                                = databricks_secret.spn_app_id.config_reference
        "SPN_APP_SECRET"                            = databricks_secret.spn_app_secret.config_reference
        "DATALAKE_STORAGE_ACCOUNT"                  = module.st_measurements.name
        "CONTINUOUS_STREAMING_ENABLED"              = var.enable_continuous_streaming
        "BRONZE_CONTAINER_NAME"                     = azurerm_storage_container.bronze.name
        "SILVER_CONTAINER_NAME"                     = azurerm_storage_container.silver.name
        "GOLD_CONTAINER_NAME"                       = azurerm_storage_container.gold.name
        "BRONZE_DATABASE_NAME"                      = databricks_schema.measurements_bronze.name
        "SILVER_DATABASE_NAME"                      = databricks_schema.measurements_silver.name
        "GOLD_DATABASE_NAME"                        = databricks_schema.measurements_gold.name
      }
    }
  }

  task {
    task_key        = "silver_notify_transactions_persisted_stream_task"
    max_retries     = 1
    job_cluster_key = "silver_notify_transactions_persisted_stream_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/core/core-0.1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "core"
      # The entry point is defined in pyproject.toml
      entry_point = "notify_transactions_persisted"
    }
  }

  email_notifications {
    no_alert_for_skipped_runs = false
  }

  queue {
    enabled = false
  }
}

resource "databricks_permissions" "silver_notify_transactions_persisted_stream" {
  provider = databricks.dbw
  job_id   = databricks_job.silver_notify_transactions_persisted_stream.id

  access_control {
    group_name       = var.databricks_contributor_dataplane_group.name
    permission_level = "CAN_MANAGE"
  }
  dynamic "access_control" {
    for_each = local.readers
    content {
      group_name       = access_control.key
      permission_level = "CAN_VIEW"
    }
  }
}
