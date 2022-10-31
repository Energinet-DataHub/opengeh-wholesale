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

# Create our own representation of a shared keyvault in development
# Then add shared secrets to this key vault an all other Terraform code in main should not need to care about the
# "local shared" key vault concept.

/*
This is a key vault provisioned in development environments in order to eliminate external dependencies.
"stub" signals that we are stub'ing away shared resources and other domains.
We ignore the settings (variables 'sharedresources_keyvault_name' and 'sharedresources_resource_group_name').

Pros and cons for using/provisioning a shared key vault stub:

Pros:
- We don't need to override e.g. azfunc appsettings
- Development environments works as similar to other environments as possible (by actually using a dedicated key vault for system-wide settings)
- We can add secrets (e.g. connection strings that are desired to be accessed for e.g. testing using Postman or other semi-automated tests requiring the secrets)

Cons:
- We could have simplified some things and saved provisioning this addition resource by completely eliminating the use of a key vault and simply
  overriding e.g. azfunc appsettings
*/

module "kv_shared_stub" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault?ref=2.0.0"
  name                            = "kvsharedst${var.project}${var.organisation}${var.environment}"
  resource_group_name             = azurerm_resource_group.main.name
  location                        = azurerm_resource_group.main.location
  tags                            = azurerm_resource_group.main.tags
  enabled_for_template_deployment = true
  sku_name                        = "standard"
  
  access_policy = [
    {
      tenant_id               = var.tenant_id
      object_id               = var.spn_object_id
      secret_permissions      = ["set", "get", "list", "delete"]
      certificate_permissions = []
      key_permissions         = []
      storage_permissions     = []
    }
  ]
}

module "kvs_integrationevents_listener_connection_string" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=2.0.0"
  name          = local.INTEGRATION_EVENTS_LISTENER_CONNECTION_STRING
  value         = module.sbnar_integrationevents_listener.primary_connection_string
  key_vault_id  = module.kv_shared_stub.id
}

module "kvs_integrationevents_sender_connection_string" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=2.0.0"
  name          = local.INTEGRATION_EVENTS_SENDER_CONNECTION_STRING
  value         = module.sbnar_integrationevents_sender.primary_connection_string
  key_vault_id  = module.kv_shared_stub.id
}

module "kvs_integrationevents_manager_connection_string" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=2.0.0"
  name          = local.INTEGRATION_EVENTS_MANAGER_CONNECTION_STRING
  value         = module.sbnar_integrationevents_manager.primary_connection_string
  key_vault_id  = module.kv_shared_stub.id
}

module "kvs_messagehub_storage_connection_string" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=2.0.0"
  name          = local.MESSAGEHUB_STORAGE_CONNECTION_STRING_KEY
  value         = module.st_marketoperator_response.primary_connection_string
  key_vault_id  = module.kv_shared_stub.id
}

module "kvs_messagehub_storage_container" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=2.0.0"
  name          = local.MESSAGEHUB_STORAGE_CONTAINER_KEY
  value         = module.container_postoffice_reply.name
  key_vault_id  = module.kv_shared_stub.id
}
