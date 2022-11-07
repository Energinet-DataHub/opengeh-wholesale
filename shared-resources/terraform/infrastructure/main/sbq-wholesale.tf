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
module "sbq_wholesale" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=7.0.0"

  name                = "wholesale"
  namespace_id        = module.sb_domain_relay.id
  requires_session    = true
}

module "sbq_wholesale_reply" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=7.0.0"

  name                = "wholesale-reply"
  namespace_id        = module.sb_domain_relay.id
  requires_session    = true
}

module "sbq_wholesale_dequeue" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=7.0.0"

  name                = "wholesale-dequeue"
  namespace_id        = module.sb_domain_relay.id
}

module "kvs_sbq_wholesale_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=7.0.0"

  name          = "sbq-wholesale-name"
  value         = module.sbq_wholesale.name
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_sbq_wholesale_reply_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=7.0.0"

  name          = "sbq-wholesale-reply-name"
  value         = module.sbq_wholesale_reply.name
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_sbq_wholesale_dequeue_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=7.0.0"

  name          = "sbq-wholesale-dequeue-name"
  value         = module.sbq_wholesale_dequeue.name
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}
