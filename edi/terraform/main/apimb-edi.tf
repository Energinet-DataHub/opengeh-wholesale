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
resource "azurerm_api_management_backend" "edi" {
  name                = "edi"
  resource_group_name = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name = data.azurerm_key_vault_secret.apim_instance_name.value
  protocol            = "http"
  url                 = "https://${module.func_receiver.default_hostname}"
}
