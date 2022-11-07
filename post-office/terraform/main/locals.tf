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
locals {
  message_db_connection_string    = "AccountEndpoint=${module.cosmos_messages.endpoint}/;AccountKey=${module.cosmos_messages.primary_key};"
  sql_actor_db_connection_string  = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${data.azurerm_key_vault_secret.mssql_market_participant_database_name.value};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.mssql_data_admin_name.value};Password=${data.azurerm_key_vault_secret.mssql_data_admin_password.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  FLOW_LOG_ENABLED                = false
}
