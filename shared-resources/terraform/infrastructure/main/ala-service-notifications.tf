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

resource "azurerm_monitor_activity_log_alert" "main" {
  name                = "ala-shared-servicenotifications-${lower(var.environment_short)}-${var.environment_instance}"
  resource_group_name = azurerm_resource_group.this.name

  scopes              = [
    data.azurerm_subscription.this.id
  ]
  description         = "Alert will fire in case of a service issue or planned maintenance of any resources in the subscription."

  criteria {
    category          = "ServiceHealth"

    service_health {
      events          = [
        "Incident",
        "Maintenance"
      ]
    }
  }

  action {
    action_group_id   = module.ag_primary.id
  }

  lifecycle {
    ignore_changes    = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }
}