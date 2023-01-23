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
data "azuread_client_config" "current" {}

resource "azuread_application" "app_databricks" {
  display_name = "sp-dbw-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  owners       = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_service_principal" "spn_databricks" {
  application_id               = azuread_application.app_databricks.application_id
  app_role_assignment_required = false
  owners                       = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_application_password" "secret" {
  application_object_id = azuread_application.app_databricks.object_id
}

module "kvs_app_databricks_password" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "spn-databricks-secret"
  value         = azuread_application_password.secret.value
  key_vault_id  = module.kv_internal.id
}