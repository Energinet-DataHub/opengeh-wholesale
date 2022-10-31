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
data "template_file" "dash_charges_template" {
  template = file("${path.module}/dash-charges-template.json")
  vars = {
    subscription_id          = data.azurerm_subscription.this.subscription_id
    resouce_group_name       = azurerm_resource_group.this.name
    application_insight_name = data.azurerm_key_vault_secret.appi_shared_name.value
    service_bus_namespace    = data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value
  }
}

resource "azurerm_portal_dashboard" "dash_charges" {
  name                  = "dash-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name   = azurerm_resource_group.this.name
  location              = azurerm_resource_group.this.location
  dashboard_properties  = data.template_file.dash_charges_template.rendered
  
  tags                  = azurerm_resource_group.this.tags

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }
}
