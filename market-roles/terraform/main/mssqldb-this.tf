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
data "azurerm_mssql_server" "mssqlsrv" {
  name                = data.azurerm_key_vault_secret.mssql_data_name.value
  resource_group_name = data.azurerm_resource_group.shared_resources.name
}

module "mssqldb_marketroles" {
  source                      = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-database?ref=v9"

  name                        = "marketroles"
  project_name                = var.domain_name_short
  environment_short           = var.environment_short
  environment_instance        = var.environment_instance  
  server_id                   = data.azurerm_mssql_server.mssqlsrv.id
  log_analytics_workspace_id  = data.azurerm_key_vault_secret.log_shared_id.value
  sql_server_name             = data.azurerm_mssql_server.mssqlsrv.name
  elastic_pool_id             = data.azurerm_key_vault_secret.mssql_data_elastic_pool_id.value
  
  tags                        = data.azurerm_resource_group.shared_resources.tags
}
