resource "databricks_job" "calculated_measurements_database_migration" {
  provider            = databricks.dbw
  name                = "Calculated Measurements Database Migration"
  max_concurrent_runs = 1

  job_cluster {
    job_cluster_key = "calculated_measurements_database_migration_cluster"

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
        "CATALOG_NAME"                              = data.azurerm_key_vault_secret.shared_unity_catalog_name.value
        "APPLICATIONINSIGHTS_CONNECTION_STRING"     = data.azurerm_key_vault_secret.appi_shared_connection_string.value
        "MEASUREMENTS_CALCULATED_INTERNAL_DATABASE" = local.database_measurements_calculated_internal
        "MEASUREMENTS_CALCULATED_DATABASE"          = local.measurements_calculated
      }
    }
  }

  task {
    task_key        = "calculated_measurements_database_migration_task"
    max_retries     = 1
    job_cluster_key = "calculated_measurements_database_migration_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/geh_calculated_measurements/geh_calculated_measurements-0.1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "geh_calculated_measurements"
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

resource "databricks_permissions" "calculated_measurements_database_migration" {
  provider = databricks.dbw
  job_id   = databricks_job.calculated_measurements_database_migration.id

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
