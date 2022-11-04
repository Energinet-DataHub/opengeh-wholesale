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
module "message_archive" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/cosmos-db-account?ref=7.0.0"
  name                                      = "logs"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  private_endpoint_subnet_id                = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
}

resource "azurerm_cosmosdb_sql_database" "db" {
  name                = "message-archive"
  resource_group_name = azurerm_resource_group.this.name
  account_name        = module.message_archive.name
}

resource "azurerm_cosmosdb_sql_container" "collection_catalog" {
  name                = "logs"
  resource_group_name = var.resource_group_name
  account_name        = module.message_archive.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/partitionKey"
}