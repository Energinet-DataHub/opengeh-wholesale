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
module "log_workspace_shared" {
  source               = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/log-workspace?ref=v10"
  name                 = "shared"
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  sku                  = "PerGB2018"
  retention_in_days    = var.log_retention_in_days
  project_name         = var.domain_name_short
}

module "kvs_log_shared_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "log-shared-name"
  value         = module.log_workspace_shared.name
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_log_shared_id" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "log-shared-id"
  value         = module.log_workspace_shared.id
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}