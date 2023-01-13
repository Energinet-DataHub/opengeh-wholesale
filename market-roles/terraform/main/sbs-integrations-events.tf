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
module "sbtsub_wholesale_process_completed_event_listener" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v10"
  name                = local.WHOLESALE_PROCESS_COMPLETED_EVENT_SUBSCRIPTION_NAME
  topic_id            = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_id.value
  project_name        = var.domain_name_short
  max_delivery_count  = 1 
  correlation_filter  = {
    label   = local.WHOLESALE_PROCESS_COMPLETED_EVENT_TYPE_NAME
  }
}