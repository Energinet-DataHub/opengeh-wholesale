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
resource "azurerm_monitor_scheduled_query_rules_alert" "integration_events_persister_streaming_job_alert" {
  name                      = "alert-integration-events-persister-streaming-job-failed-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  location                  = azurerm_resource_group.this.location
  resource_group_name       = var.shared_resources_resource_group_name
  
  action {
    action_group = [data.azurerm_key_vault_secret.primary_action_group_id.value]
  }

  data_source_id            = data.azurerm_key_vault_secret.log_shared_id.value
  description               = "Integration events persister streaming has failed"
  enabled                   = true
  query                     = <<-QUERY
  DatabricksJobs
| where OperationName == "Microsoft.Databricks/jobs/runFailed"
| where parse_json(RequestParams).taskKey startswith "integration_events_persister_streaming_job"
  QUERY
  severity                  = 1
  frequency                 = 60
  time_window               = 60
  
  trigger {
    operator                = "GreaterThan"
    threshold               = 3
  }
}