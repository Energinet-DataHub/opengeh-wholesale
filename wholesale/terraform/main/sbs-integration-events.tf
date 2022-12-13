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

module "sbs_int_events_grid_area_updated" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v10"
  name                = "grid-area-updated"
  project_name        = var.domain_name_short
  topic_id            = data.azurerm_key_vault_secret.sbt_integration_events_id.value
  max_delivery_count  = 10
  correlation_filter  = {
    properties     = {
      "messageType" = "GridAreaUpdatedIntegrationEvent"
    }
  }
}
