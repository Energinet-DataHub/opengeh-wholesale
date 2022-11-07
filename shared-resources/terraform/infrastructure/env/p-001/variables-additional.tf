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
variable sas_token_datamig_charges {
  type        = string
  description = "The SAS token for the storage account containing migrated DataHub 2 data dedicated to the Charges domain"
}

variable sas_token_datamig_market_roles {
  type        = string
  description = "The SAS token for the storage account containing migrated DataHub 2 data dedicated to the Market roles domain"
}

variable sas_token_datamig_metering_point {
  type        = string
  description = "The SAS token for the storage account containing migrated DataHub 2 data dedicated to the Metering point domain"
}

variable sas_token_datamig_time_series {
  type        = string
  description = "The SAS token for the storage account containing migrated DataHub 2 data dedicated to the Time series domain"
}

variable sas_token_datamig_wholesale {
  type        = string
  description = "The SAS token for the storage account containing migrated DataHub 2 data dedicated to the Wholesale domain"
}