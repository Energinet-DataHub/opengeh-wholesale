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
  namespace_id        = data.azurerm_key_vault_secret.sb_domainrelay_namespace_id.value
 
  subscriptions       = [
    {
      name                = local.COMPLETED_BATCH_SUBSCRIPTION_ZIP_BASIS_DATA
      max_delivery_count  = 1
    },
    {
      name                = local.COMPLETED_BATCH_SUBSCRIPTION_PUBLISH_PROCESSES_COMPLETED
      max_delivery_count  = 1
    },
    {
      name                = local.COMPLETED_PROCESS_SUBSCRIPTION_SEND_DATA_AVAILABLE
      max_delivery_count  = 1
    },
  ]
}

module "sbtsub-charges-info-operations-accepted" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "info-operations-accepted"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeInformationOperationsAcceptedEvent"
  }
}