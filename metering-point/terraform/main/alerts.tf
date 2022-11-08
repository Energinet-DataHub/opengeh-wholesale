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
resource "azurerm_monitor_action_group" "metering_point" {
  name                      = "ag-metering-point-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name       = azurerm_resource_group.this.name
  short_name                = "ag-mp-${lower(var.environment_short)}-${lower(var.environment_instance)}"

  email_receiver {
    name                    = "Alerts-metering-point-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
    email_address           = "0a494d0d.energinet.onmicrosoft.com@emea.teams.ms"
    use_common_alert_schema = true
  }
}


resource "azurerm_monitor_scheduled_query_rules_alert" "metering_point_alert" {
  name                      = "alert-metering-point-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  location                  = azurerm_resource_group.this.location
  resource_group_name       = var.shared_resources_resource_group_name

  action {
    action_group            = [azurerm_monitor_action_group.metering_point.id]
  }
  data_source_id            = data.azurerm_key_vault_secret.appi_shared_id.value
  description               = "Alert when total results cross threshold"
  enabled                   = true
  query                     = <<-QUERY
    requests
| where timestamp > ago(10m) and  success == false
| join kind= inner (
exceptions
| where timestamp > ago(10m)
  and (cloud_RoleName == 'func-ingestion-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}'
  or cloud_RoleName == 'func-outbox-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}'
  or cloud_RoleName == 'func-localmessagehub-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}'
  or cloud_RoleName == 'func-processing-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}'
  or cloud_RoleName == 'func-actorsync-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}')
) on operation_Id
| project exceptionType = type, failedMethod = method, requestName = name, requestDuration = duration, function = cloud_RoleName
  QUERY
  severity                  = 1
  frequency                 = 5
  time_window               = 10
  trigger {
    operator                = "GreaterThan"
    threshold               = 0
  }
}

resource "azurerm_monitor_scheduled_query_rules_alert" "metering_point_internal_commands_alert" {
  name                      = "alert-internal-commands-metering-point-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  location                  = azurerm_resource_group.this.location
  resource_group_name       = var.shared_resources_resource_group_name

  action {
    action_group            = [azurerm_monitor_action_group.metering_point.id]
  }
  data_source_id            = data.azurerm_key_vault_secret.appi_shared_id.value
  description               = "One or more metering point internal commands couldn't be processed."
  enabled                   = true
  query                     = <<-QUERY
  traces
| where timestamp > ago(60m)
| where message has "Failed to process internal command"
and sdkVersion has "azurefunctions"
and cloud_RoleName has "${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
| order by timestamp desc
  QUERY
  severity                  = 1
  frequency                 = 60
  time_window               = 60
  trigger {
    operator                = "GreaterThan"
    threshold               = 0
  }
}