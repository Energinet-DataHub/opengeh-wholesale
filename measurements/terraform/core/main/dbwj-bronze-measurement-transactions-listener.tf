resource "databricks_job" "bronze_submitted_transactions_ingestion_stream" {
  provider            = databricks.dbw
  name                = "Bronze Submitted Transactions Ingestion Stream"
  max_concurrent_runs = 1

  job_cluster {
    job_cluster_key = "bronze_submitted_transactions_ingestion_stream_cluster"

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
        "EVENT_HUB_INSTANCE"                        = data.azurerm_key_vault_secret.evh_measurement_transactions_name.value
        "TENANT_ID"                                 = var.tenant_id,
        "SPN_APP_ID"                                = databricks_secret.spn_app_id.config_reference
        "SPN_APP_SECRET"                            = databricks_secret.spn_app_secret.config_reference
        "CONTINUOUS_STREAMING_ENABLED"              = var.enable_continuous_streaming
      }
    }
  }

  task {
    task_key        = "bronze_submitted_transactions_ingestion_stream_task"
    max_retries     = 1
    job_cluster_key = "bronze_submitted_transactions_ingestion_stream_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/core/bronze-0.1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "opengeh_bronze"
      # The entry point is defined in pyproject.toml
      entry_point = "ingest_submitted_transactions"
    }
  }

  email_notifications {
    no_alert_for_skipped_runs = false
  }

  queue {
    enabled = false
  }
}

resource "databricks_permissions" "bronze_submitted_transactions_ingestion_stream" {
  provider = databricks.dbw
  job_id   = databricks_job.bronze_submitted_transactions_ingestion_stream.id

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
