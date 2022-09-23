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
resource "azurerm_monitor_action_group" "wholesales" {
  name                      = "ag-wholesales-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name       = azurerm_resource_group.this.name
  short_name                = "ag-ws-${lower(var.environment_short)}-${lower(var.environment_instance)}"

  email_receiver {
    name                    = "Alerts-Wholesales-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
    email_address           = "0e195df3.energinet.onmicrosoft.com@emea.teams.ms"
    use_common_alert_schema = true
  }
}

resource "azurerm_monitor_scheduled_query_rules_alert" "wholesales_databricks_job_alert" {
  name                      = "alert-wholesales-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  location                  = azurerm_resource_group.this.location
  resource_group_name       = var.shared_resources_resource_group_name

  action {
    action_group            = [azurerm_monitor_action_group.wholesales.id]
  }
  data_source_id            = data.azurerm_key_vault_secret.log_shared_id.value
  description               = "One or more databricks jobs has failed"
  enabled                   = true
  query                     = <<-QUERY
  DatabricksJobs 
| where OperationName == "Microsoft.Databricks/jobs/runFailed"
  QUERY
  severity                  = 1
  frequency                 = 5
  time_window               = 5
  trigger {
    operator                = "GreaterThan"
    threshold               = 0
  }
}