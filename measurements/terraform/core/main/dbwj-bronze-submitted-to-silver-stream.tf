resource "databricks_job" "bronze_submitted_transactions_to_silver" {
  provider            = databricks.dbw
  name                = "Bronze Submitted Transactions to Silver Measurements"
  max_concurrent_runs = 1

  tags = {
    owner       = "Team Volt"
  }
  job_cluster {
    job_cluster_key = "bronze_to_silver_cluster"

    new_cluster {
      spark_version  = local.spark_version
      node_type_id   = "Standard_D8ads_v5"
      runtime_engine = "PHOTON"
      autoscale {
        min_workers = 1
        max_workers = 1
      }
      spark_conf = {
        "spark.databricks.sql.initial.catalog.name" = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
      }

      spark_env_vars = {
        "CATALOG_NAME"                          = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
        "APPLICATIONINSIGHTS_CONNECTION_STRING" = data.azurerm_key_vault_secret.appi_shared_connection_string.value
        "DATALAKE_STORAGE_ACCOUNT"              = module.st_measurements.name
        "CONTINUOUS_STREAMING_ENABLED"              = local.enable_continuous_streaming
        "BRONZE_DATABASE_NAME"                      = databricks_schema.measurements_bronze.name
        "BRONZE_CONTAINER_NAME"                     = azurerm_storage_container.bronze.name
        "SILVER_DATABASE_NAME"                      = databricks_schema.measurements_silver.name
        "SILVER_CONTAINER_NAME"                     = azurerm_storage_container.silver.name
      }
    }
  }

  task {
    task_key        = "stream_and_transform"
    max_retries     = 1
    job_cluster_key = "bronze_to_silver_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/core/core-0.1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "core"
      # The entry point is defined in pyproject.toml
      entry_point  = "stream_submitted_transactions_to_silver"
    }
  }
}

resource "databricks_permissions" "bronze_submitted_transactions_to_silver" {
  provider = databricks.dbw
  job_id   = databricks_job.bronze_submitted_transactions_to_silver.id

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
