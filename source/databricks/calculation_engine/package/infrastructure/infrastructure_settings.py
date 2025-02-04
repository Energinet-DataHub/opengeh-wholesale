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
import os
from dataclasses import dataclass, field
from pydantic_settings import BaseSettings, CliSettingsSource, PydanticBaseSettingsSource
from pydantic import Field
from typing import Tuple, Type, Optional
from azure.identity import ClientSecretCredential
from package.infrastructure import paths

# @dataclass
# class InfrastructureSettings:
#     catalog_name: str
#     calculation_input_database_name: str
#     data_storage_account_name: str
#     # Prevent the credentials from being printed or logged (using e.g. print() or repr())
#     data_storage_account_credentials: ClientSecretCredential = field(repr=False)
#     wholesale_container_path: str
#     calculation_input_path: str
#     time_series_points_table_name: str | None
#     metering_point_periods_table_name: str | None
#     grid_loss_metering_point_ids_table_name: str | None

class InfrastructureSettings(BaseSettings):
    """
    InfrastructureSettings class uses Pydantic BaseSettings to configure and validate parameters.
    Parameters can come from both runtime (CLI) or from environment variables.
    The priority is CLI parameters first and then environment variables.
    """
    catalog_name: str #From ENVIRONMENT
    calculation_input_database_name: str #From ENVIRONMENT
    data_storage_account_name: str #From ENVIRONMENT
    # Prevent the credentials from being printed or logged (using e.g., print() or repr())
    tenant_id: Optional[str] = Field(repr=False, default="tenant_id") #Ud? #From ENVIRONMENT
    spn_app_id: Optional[str] = Field(repr=False, default="spn_app_id") #Ud? #From ENVIRONMENT
    spn_app_secret: Optional[str] = Field(repr=False, default="spn_app_secret") #Ud? #From ENVIRONMENT

    calculation_input_folder_name: Optional[str] = Field(default=None) #Ud? From CLI/ENVIRONMENT

    time_series_points_table_name: Optional[str] = None #From CLI
    metering_point_periods_table_name: str | None  = Field(default=None) #From CLI
    grid_loss_metering_point_ids_table_name: str | None = Field(default=None) #From CLI

    data_storage_account_credentials: Optional[ClientSecretCredential] = Field(default=None, repr=False)  # Filled out in model_post_init
    wholesale_container_path: Optional[str] = Field(default=None)  # Filled out in model_post_init
    calculation_input_path: Optional[str] = Field(default=None)  # Filled out in model_post_init


    # def model_post_init(self, __context):
    #     """Automatically set data_storage_account_credentials after settings are loaded."""
    #     self.data_storage_account_credentials = ClientSecretCredential(
    #         tenant_id=str(self.tenant_id),
    #         client_id=str(self.spn_app_id),
    #         client_secret=str(self.spn_app_secret),
    #     )
    #     self.wholesale_container_path = paths.get_container_root_path(str(self.data_storage_account_name))
    #     self.calculation_input_path = paths.get_calculation_input_path(str(self.data_storage_account_name), str(self.calculation_input_folder_name))
    #

    @classmethod
    def settings_customise_sources(
        cls,
        settings_cls: Type[BaseSettings],
        init_settings: PydanticBaseSettingsSource,
        env_settings: PydanticBaseSettingsSource,
        dotenv_settings: PydanticBaseSettingsSource,
        file_secret_settings: PydanticBaseSettingsSource,
    ) -> Tuple[PydanticBaseSettingsSource, ...]:
        return CliSettingsSource(settings_cls, cli_parse_args=True, cli_ignore_unknown_args=True), env_settings, init_settings







# os.environ['CATALOG_NAME'] = 'catalog_name_str'
# os.environ['CALCULATION_INPUT_DATABASE_NAME'] = 'calculation_input_database_name_str'
# os.environ['DATA_STORAGE_ACCOUNT_NAME'] = 'data_storage_account_name_str'
# os.environ['TENANT_ID'] = '550e8400-e29b-41d4-a716-446655440000'
# os.environ['SPN_APP_ID'] = '123e4567-e89b-12d3-a456-426614174000'
# os.environ['SPN_APP_SECRET'] = 'MyPassword~HQ'
# os.environ['CALCULATION_INPUT_FOLDER_NAME'] = 'calculation_input_folder_name_env'
#
#
#
#
#
# print(_settings.catalog_name)
# #
# for s in _settings:
#     print(s)
# #
# print(isinstance(_settings.data_storage_account_credentials, ClientSecretCredential))
# # # Inspect the attributes of the ClientSecretCredential object
# print(vars(_settings.data_storage_account_credentials))

