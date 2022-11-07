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
module "cosmos_messages" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/cosmos-db-account?ref=7.0.0"

  name                                      = "messages"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  private_endpoint_subnet_id                = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
}

resource "azurerm_cosmosdb_sql_database" "db" {
  name                = "post-office"
  resource_group_name = azurerm_resource_group.this.name
  account_name        = module.cosmos_messages.name
}

resource "azurerm_cosmosdb_sql_container" "collection_actor" {
  name                = "actor"
  resource_group_name = var.resource_group_name
  account_name        = module.cosmos_messages.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/partitionKey"
}

resource "azurerm_cosmosdb_sql_container" "collection_catalog" {
  name                = "catalog"
  resource_group_name = azurerm_resource_group.this.name
  account_name        = module.cosmos_messages.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/partitionKey"
}

resource "azurerm_cosmosdb_sql_container" "collection_cabinet" {
  name                = "cabinet"
  resource_group_name = azurerm_resource_group.this.name
  account_name        = module.cosmos_messages.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/partitionKey"
}

resource "azurerm_cosmosdb_sql_container" "collection_idempotency" {
  name                = "idempotency"
  resource_group_name = azurerm_resource_group.this.name
  account_name        = module.cosmos_messages.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/partitionKey"
}

resource "azurerm_cosmosdb_sql_container" "collection_bundle" {
  name                = "bundle"
  resource_group_name = azurerm_resource_group.this.name
  account_name        = module.cosmos_messages.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/recipient"
}

resource "azurerm_cosmosdb_sql_trigger" "triggers_ensuresingleunacknowledgedbundle" {
  name         = "EnsureSingleUnacknowledgedBundle"
  container_id = azurerm_cosmosdb_sql_container.collection_bundle.id
  body         = <<EOT
    function trigger() {

    var context = getContext();
    var container = context.getCollection();
    var response = context.getResponse();
    var createdItem = response.getBody();

    var filterQuery = `
    SELECT * FROM bundle b
    WHERE b.recipient = '$${createdItem.recipient}' AND
          b.dequeued = false AND (
          b.origin = '$${createdItem.origin}' OR
         ((b.origin = 'MarketRoles' OR b.origin = 'Charges') AND '$${createdItem.origin}' = 'MeteringPoints') OR
         ((b.origin = 'Charges' OR b.origin = 'MeteringPoints') AND '$${createdItem.origin}' = 'MarketRoles') OR
         ((b.origin = 'MarketRoles' OR b.origin = 'MeteringPoints') AND '$${createdItem.origin}' = 'Charges'))`;

    var accept = container.queryDocuments(container.getSelfLink(), filterQuery, function(err, items, options)
    {
        if (err) throw err;
        if (items.length !== 0) throw 'SingleBundleViolation';
    });

    if (!accept) throw 'queryDocuments in trigger failed.'; }
      EOT
  operation    = "Create"
  type         = "Post"
}