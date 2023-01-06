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
module "kv_shared_access_policy_func_entrypoint_marketparticipant" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-access-policy?ref=v10"

  key_vault_id              = data.azurerm_key_vault.kv_shared_resources.id
  app_identity              = module.func_entrypoint_marketparticipant.identity.0
}

resource "azurerm_key_vault_access_policy" "kv_shared_access_policy_app_webapi" {
  key_vault_id            = data.azurerm_key_vault.kv_shared_resources.id
  tenant_id               = module.app_webapi.identity.0.tenant_id
  object_id               = module.app_webapi.identity.0.principal_id
  secret_permissions      = [
    "List",
    "Get"
  ]
  key_permissions         = [
    "Get",
    "List",
    "Sign"
  ]
}
