resource "databricks_job" "silver_to_gold_measurements" {
  provider            = databricks.dbw
  name                = "Silver to Gold Measurements"
  max_concurrent_runs = 1

  job_cluster {
    job_cluster_key = "silver_to_gold_measurements_cluster"

    new_cluster {
      spark_version  = local.spark_version
      node_type_id   = "Standard_D8as_v4"
      runtime_engine = "PHOTON"
      autoscale {
        min_workers = 1
        max_workers = 5
      }
      spark_conf = {
        "spark.databricks.sql.initial.catalog.name" = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
      }

      spark_env_vars = {
        "CATALOG_NAME"                          = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
        "APPLICATIONINSIGHTS_CONNECTION_STRING" = data.azurerm_key_vault_secret.appi_shared_connection_string.value
        "DATALAKE_STORAGE_ACCOUNT"              = module.st_measurements.name
        "SILVER_CONTAINER_NAME"                     = azurerm_storage_container.silver.name
        "GOLD_CONTAINER_NAME"                       = azurerm_storage_container.gold.name
        "SILVER_DATABASE_NAME"                      = databricks_schema.measurements_silver.name
        "GOLD_DATABASE_NAME"                        = databricks_schema.measurements_gold.name
      }
    }
  }

  task {
    task_key        = "silver_to_gold_measurements_task"
    max_retries     = 1
    job_cluster_key = "silver_to_gold_measurements_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/core/core-0.1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "core"
      # The entry point is defined in pyproject.toml
      entry_point  = "stream_silver_to_gold_measurements"
    }
  }
}

resource "databricks_permissions" "silver_to_gold_measurements" {
  provider = databricks.dbw
  job_id   = databricks_job.silver_to_gold_measurements.id

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
