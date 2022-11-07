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
module "sbt_charge_link_created" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic?ref=7.0.0"

  name                = "charge-link-created"
  namespace_id        = module.sb_domain_relay.id
  subscriptions       = [
    {
      name                = "charge-link-created-sub-charges"
      max_delivery_count  = 1
    },
    {
      name                = "charge-link-created-forward",
      max_delivery_count  = 1,
      forward_to          = module.sbt_domainrelay_integrationevent_received.name
    },        
  ]
}

module "kvs_sbt_charge_link_created_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=7.0.0"

  name          = "sbt-charge-link-created-name"
  value         = module.sbt_charge_link_created.name
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}
