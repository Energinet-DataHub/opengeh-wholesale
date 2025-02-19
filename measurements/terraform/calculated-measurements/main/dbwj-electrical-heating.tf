resource "databricks_job" "electrical_heating" {
  provider            = databricks.dbw
  name                = "ElectricalHeating"
  max_concurrent_runs = 1

  job_cluster {
    job_cluster_key = "electrical_heating_cluster"

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
        "ELECTRICITY_MARKET_DATA_PATH"          = "/Volumes/${data.azurerm_key_vault_secret.shared_unity_catalog_name.value}/measurements_internal/shared_electricity_market_electrical_heating_container"
      }
    }
  }

  task {
    task_key        = "electrical_heating_task"
    max_retries     = 1
    job_cluster_key = "electrical_heating_cluster"

    library {
      whl = "/Workspace/Shared/PythonWheels/electrical_heating/geh_calculated_measurements-0.1.0-py3-none-any.whl"
    }

    python_wheel_task {
      package_name = "geh_calculated_measurements"
      # The entry point is defined in setup.py
      entry_point = "execute_electrical_heating"
    }
  }

  email_notifications {
    no_alert_for_skipped_runs = false
  }

  queue {
    enabled = false
  }
}

resource "databricks_permissions" "electrical_heating" {
  provider = databricks.dbw
  job_id   = databricks_job.electrical_heating.id

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
