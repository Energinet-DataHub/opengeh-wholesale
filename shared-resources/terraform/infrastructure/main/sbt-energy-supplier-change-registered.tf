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
module "sbt_energy_supplier_change_registered" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic?ref=7.0.0"

  name                = "energy-supplier-change-registered"
  namespace_id        = module.sb_domain_relay.id
  subscriptions       = [
    {
      name                = "energy-supplier-change-registered-forward",
      max_delivery_count  = 1,
      forward_to          = module.sbt_domainrelay_integrationevent_received.name
    },       
  ]
}
