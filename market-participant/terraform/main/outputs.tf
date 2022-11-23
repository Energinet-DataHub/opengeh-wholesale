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
output ms_market_participant_connection_string {
  description = "Connection string of the market participant database created in the shared server"
  value       = local.MS_MARKET_PARTICIPANT_CONNECTION_STRING_SQL_AUTH
  sensitive   = true
}

output ms_market_participant_database_name {
  description = "Database name in the shared sql server"
  value = module.mssqldb_market_participant.name
  sensitive = true  
}

output ms_market_participant_database_server {
  description = "Database server instance hosting the Market Participant database"
  value = data.azurerm_key_vault_secret.mssql_data_url.value
  sensitive = true  
}