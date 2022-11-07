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
module "appi_shared" {
  source                = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/application-insights?ref=7.0.0"

  name                         = "shared"
  project_name                 = var.domain_name_short
  environment_short            = var.environment_short
  environment_instance         = var.environment_instance
  resource_group_name          = azurerm_resource_group.this.name
  location                     = azurerm_resource_group.this.location
  log_analytics_workspace_id   = module.log_workspace_shared.id 
  tags                         = azurerm_resource_group.this.tags
}

module "kvs_appi_shared_instrumentation_key" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=7.0.0"

  name          = "appi-shared-instrumentation-key"
  value         = module.appi_shared.instrumentation_key
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_appi_shared_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=7.0.0"

  name          = "appi-shared-name"
  value         = module.appi_shared.name
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_appi_shared_id" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=7.0.0"

  name          = "appi-shared-id"
  value         = module.appi_shared.id
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}
