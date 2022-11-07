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
locals {
  SBS_CONSUMER_MOVED_IN_NAME = "consumer-moved-in"
}

module "sbt_consumer_moved_in" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic?ref=7.0.0"
    
  name          = "consumer-moved-in"
  namespace_id  = module.sb_domain_relay.id
  subscriptions = [
    {
      name = local.SBS_CONSUMER_MOVED_IN_NAME
      max_delivery_count = 10
    },
    {
      name                = "consumer-moved-in-forward",
      max_delivery_count  = 1,
      forward_to          = module.sbt_domainrelay_integrationevent_received.name
    },          
  ]
}

module "kvs_sbt_consumer_moved_in_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=7.0.0"

  name          = "sbt-consumer-moved-in-name"
  value         = module.sbt_consumer_moved_in.name
  key_vault_id  = module.kv_shared.id
    
  tags          = azurerm_resource_group.this.tags
}

module "kvs_sbs_consumer_moved_in_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=7.0.0"

  name          = "sbs-consumer-moved-in-name"
  value         = local.SBS_CONSUMER_MOVED_IN_NAME
  key_vault_id  = module.kv_shared.id
    
  tags          = azurerm_resource_group.this.tags
}