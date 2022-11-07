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

module "kv_access_policy_developers_security_group" {
  count         = var.developers_security_group_object_id == null ? 0 : 1
  
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=7.0.0"

  key_vault_id  = module.kv_shared.id
  app_identity  = {
    tenant_id     = var.arm_tenant_id
    principal_id  = var.developers_security_group_object_id
  }
}