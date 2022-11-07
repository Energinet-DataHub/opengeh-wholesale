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
resource "azurerm_api_management_backend" "metering_point" {
  name                = "metering_point"
  resource_group_name = azurerm_resource_group.this.name
  api_management_name = module.apim_shared.name
  protocol            = "http"
  url                 = var.apimao_metering_point_domain_ingestion_function_url
}