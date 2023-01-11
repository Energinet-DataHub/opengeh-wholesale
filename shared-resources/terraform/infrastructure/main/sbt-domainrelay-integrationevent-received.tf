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
module "sbt_domainrelay_integrationevent_received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic?ref=v10"

  name                = "integrationevent-received"
  namespace_id        = module.sb_domain_relay.id
  project_name        = var.domain_name_short
}

module "kvs_sbt_domainrelay_integrationevent_received_id" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "${module.sbt_domainrelay_integrationevent_received.name}-id" # name will be sbt-sharedres-integrationevent-received-id due to naming convention enforced by the TF module
  value         = module.sbt_domainrelay_integrationevent_received.id
  key_vault_id  = module.kv_shared.id
}

module "kvs_sbt_domainrelay_integrationevent_received_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "${module.sbt_domainrelay_integrationevent_received.name}-name" # name will be sbt-sharedres-integrationevent-received-name due to naming convention enforced by the TF module
  value         = module.sbt_domainrelay_integrationevent_received.name
  key_vault_id  = module.kv_shared.id
}
