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

/*
=================================================================================
Infrastructure for a representation of the queues of externally published integration events.
This is used to be able to fully explore and integration test the charges domain
without relying on the external dependencies to other domains.

In order to make it lightweight we implement as additional topics
on the existing Service Bus Namespace.
=================================================================================
*/

module "sbs_charge_prices-updated_charge" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-subscription?ref=2.0.0"
  name                = "charge-prices-updated-sub-charges"
  resource_group_name = azurerm_resource_group.main.name
  namespace_name      = module.sbn_external_integration_events.name
  topic_name          = module.sbt_charge_prices_updated.name
  max_delivery_count  = 1
  depends_on          = [module.sbt_charge_prices_updated]
}