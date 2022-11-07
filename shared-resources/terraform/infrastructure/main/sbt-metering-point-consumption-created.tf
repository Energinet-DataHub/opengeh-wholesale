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
  SBS_CONSUMPTION_METERING_POINT_SUB_CHARGES_NAME = "consumption-metering-point-created-sub-charges"
}

module "sbt_consumption_metering_point_created" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic?ref=7.0.0"

  name                = "consumption-metering-point-created"
  namespace_id        = module.sb_domain_relay.id
  subscriptions       = [
    {
      name                = local.SBS_CONSUMPTION_METERING_POINT_SUB_CHARGES_NAME
      max_delivery_count  = 1
    },
    {
      name                = "consumption-metering-point-created-forward",
      max_delivery_count  = 1,
      forward_to          = module.sbt_domainrelay_integrationevent_received.name
    },          
  ]
}

module "kvs_sbt_consumption_metering_point_created_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=7.0.0"

  name          = "sbt-consumption-metering-point-created-name"
  value         = module.sbt_consumption_metering_point_created.name
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_sbs_consumption_metering_point_created_sub_charges_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=7.0.0"

  name          = "sbs-consumption-metering-point-created-sub-charges-name"
  value         = local.SBS_CONSUMPTION_METERING_POINT_SUB_CHARGES_NAME
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}
