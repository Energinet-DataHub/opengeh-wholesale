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

output ms_wholesale_connection_string {
  description = "Connection string of the wholesale database created in the shared server"
  value       = local.DB_CONNECTION_STRING
  sensitive   = true
}

output ms_wholesale_database_name {
  description = "Database name in the shared sql server"
  value = module.mssqldb_wholesale.name
  sensitive = true  
}

output ms_wholesale_database_server {
  description = "Database server instance hosting the Wholesale database"
  value = data.azurerm_key_vault_secret.mssql_data_url.value
  sensitive = true  
}