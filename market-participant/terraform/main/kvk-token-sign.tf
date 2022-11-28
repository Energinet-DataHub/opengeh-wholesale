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
resource "azurerm_key_vault_key" "token_sign" {
  name         = "token-sign"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
  key_type     = "RSA"
  key_size     = 2048

  key_opts = [
    "sign",
  ]
}
