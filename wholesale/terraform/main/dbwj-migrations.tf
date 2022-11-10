# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

resource "databricks_job" "migrations_job" {
  name = "MigrationsJob"
  max_concurrent_runs = 1
  always_running = false

  task {
    task_key = "migrations_job_${uuid()}"
    max_retries = 1

    new_cluster {
      spark_version           = data.databricks_spark_version.latest_lts.id
      node_type_id            = "Standard_DS3_v2"
      autoscale {
        min_workers = 1
        max_workers = 4
      }
    }

    library {
      whl = "dbfs:/opengeh-wholesale/package-1.0-py3-none-any.whl"
    } 

    python_wheel_task {
      package_name = "package"
      # The entry point is defined in setup.py
      entry_point = "begin_migration"
      parameters  = [
          "--data-storage-account-name=${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}",
          "--data-storage-account-key=${data.azurerm_key_vault_secret.kvs_st_data_lake_primary_access_key.value}",
          "--integration-events-path=abfss://${local.INTERGRATION_EVENTS_CONTAINER_NAME}@${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net/events",
          "--process-results-path=abfss://${local.CALCULATION_STORAGE_CONTAINER_NAME}@${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net/results",
          "--storage-container-path=abfss://${local.STORAGE_CONTAINER_NAME}@${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net/",
          "--databricks-host=https://${data.azurerm_key_vault_secret.dbw_databricks_workspace_url.value}",
          "--databricks-token=${data.azurerm_key_vault_secret.dbw_databricks_workspace_token.value}",
          "--log-level=information"
      ]
    }
  }
  
  email_notifications {
    no_alert_for_skipped_runs = true
  }
}
