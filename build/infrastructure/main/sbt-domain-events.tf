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

module "sbt_domain_events" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic?ref=v9"

  name                = "domain-events"
  namespace_id        = data.azurerm_key_vault_secret.sb_integration_events_id.value
}

module "sbtsub_zip_basis_data_when_batch_completed" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = local.ZIP_BASIS_DATA_WHEN_COMPLETED_BATCH_SUBSCRIPTION_NAME
  project_name        = var.domain_name_short
  topic_id            = module.sbt_domain_events.id
  max_delivery_count  = 10
  correlation_filter  = {
    label = local.BATCH_COMPLETED_EVENT_NAME
  }
}

module "sbtsub_publish_process_completed_when_batch_completed" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = local.PUBLISH_PROCESSES_COMPLETED_WHEN_COMPLETED_BATCH_SUBSCRIPTION_NAME
  project_name        = var.domain_name_short
  topic_id            = module.sbt_domain_events.id
  max_delivery_count  = 10
  correlation_filter  = {
    label = local.BATCH_COMPLETED_EVENT_NAME
  }
}

module "sbtsub_send_data_available_when_process_completed" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = local.SEND_DATA_AVAILABLE_WHEN_COMPLETED_PROCESS_SUBSCRIPTION_NAME
  project_name        = var.domain_name_short
  topic_id            = module.sbt_domain_events.id
  max_delivery_count  = 10
  correlation_filter  = {
    label = local.PROCESS_COMPLETED_EVENT_NAME
  }
}
