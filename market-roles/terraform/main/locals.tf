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
  MS_MARKETROLES_CONNECTION_STRING_SQL_AUTH     = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_marketroles.name};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.mssql_data_admin_name.value};Password=${data.azurerm_key_vault_secret.mssql_data_admin_password.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  MS_MARKETROLES_CONNECTION_STRING              = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Database=${module.mssqldb_marketroles.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  ACTOR_REGISTRY_DB_CONNECTION_STRING           = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${data.azurerm_key_vault_secret.mssql_market_participant_database_name.value};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.mssql_data_admin_name.value};Password=${data.azurerm_key_vault_secret.mssql_data_admin_password.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  HUB_MRID                                      = "5790001330552"
  # Note: The following timezone name is using the naming scheme of the TZ Database. See https://en.wikipedia.org/wiki/List_of_tz_database_time_zones for list of possible values.
  LOCAL_TIMEZONENAME                            = "Europe/Copenhagen"
}