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

resource "databricks_job" "calculator_job" {
  name = "CalculatorJob"
  max_retries = 2
  max_concurrent_runs = 100
  always_running = false

  task {
    task_key = "unique_job_${uuid()}"

    new_cluster {
      spark_version           = data.databricks_spark_version.latest_lts.id
      node_type_id            = "Standard_DS3_v2"
      autoscale {
        min_workers = 1
        max_workers = 4
      }
    }

    library {
      whl = "dbfs:/package/package-1.0-py3-none-any.whl"
    } 

    python_wheel_task {
      package_name = "package"
      entry_point = "start_calculator"
      parameters  = [
          "--data-storage-account-name=${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}",
          "--data-storage-account-key=${data.azurerm_key_vault_secret.kvs_st_data_lake_primary_access_key.value}",
          "--integration-events-path=abfss://${local.INTERGRATION_EVENTS_CONTAINER_NAME}@${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net/events",
          "--market-participant-events-path=abfss://${local.MARKET_PARTICIPANT_EVENTS_CONTAINER_NAME}@${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net/events",
          "--time-series-points-path=abfss://timeseries-data@${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net/time-series-points",
          "--process-results-path=abfss://${local.PROCESSES_CONTAINER_NAME}@${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net/results",
      ]
    }
  }

  email_notifications {
    no_alert_for_skipped_runs = true
  }
}
