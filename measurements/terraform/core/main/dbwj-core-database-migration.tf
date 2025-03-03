resource "databricks_job" "core_database_migration" {
  provider            = databricks.dbw
  name                = "Core Database Migration"
  max_concurrent_runs = 1

  tags = {
    owner       = "Team Volt"
  }

  job_cluster {
    job_cluster_key = "core_database_migration_cluster"

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
        "BRONZE_CONTAINER_NAME"                     = azurerm_storage_container.bronze.name
        "SILVER_CONTAINER_NAME"                     = azurerm_storage_container.silver.name
        "GOLD_CONTAINER_NAME"                       = azurerm_storage_container.gold.name
        "BRONZE_DATABASE_NAME"                      = databricks_schema.measurements_bronze.name
        "SILVER_DATABASE_NAME"                      = databricks_schema.measurements_silver.name
        "GOLD_DATABASE_NAME"                        = databricks_schema.measurements_gold.name
        "DATABRICKS_WORKSPACE_URL"                  = module.dbw.workspace_url
        "DATABRICKS_TOKEN"                          = module.dbw.databricks_token
        "DATABRICKS_JOBS"                           = local.databricks_jobs_string
      }
    }
  }

  task {
    task_key        = "core_database_migration_task"
    max_retries     = 1
    job_cluster_key = "core_database_migration_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/core/core-0.1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "core"
      # The entry point is defined in pyproject.toml
      entry_point = "migrate"
    }
  }

  email_notifications {
    no_alert_for_skipped_runs = false
  }

  queue {
    enabled = false
  }
}

resource "databricks_permissions" "core_database_migration" {
  provider = databricks.dbw
  job_id   = databricks_job.core_database_migration.id

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
