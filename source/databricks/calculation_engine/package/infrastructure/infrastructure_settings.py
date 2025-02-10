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

from dataclasses import dataclass, field
from azure.identity import ClientSecretCredential

from pydantic_settings import BaseSettings, CliSettingsSource, PydanticBaseSettingsSource
from pydantic import AliasChoices, Field, computed_field, field_validator
from typing import Tuple, Type
from package.infrastructure import paths

class ApplicationSettings(
    BaseSettings,
    cli_parse_args=True,
    cli_kebab_case=True,
    cli_ignore_unknown_args=True,
    cli_implicit_flags=True,
):
    """
    Base class for application settings.

    Supports:
    - CLI parsing with arguments using kebab-case.
    - Environment variables using SNAKE_UPPER_CASE.
    - Ignoring unknown CLI arguments. This behavior can be overridden by setting `cli_ignore_unknown_args=False`
      in the class definition of the derived settings class. Example:
      `class Settings(ApplicationSettings, cli_ignore_unknown_args=False):`
    """

    pass

class InfrastructureSettings(ApplicationSettings, cli_kebab_case=False):
    """
    InfrastructureSettings class uses Pydantic BaseSettings to configure and validate parameters.
    Parameters can come from both runtime (CLI) or from environment variables.
    The priority is CLI parameters first and then environment variables.
    """
    catalog_name: str #From ENVIRONMENT
    calculation_input_database_name: str #From ENVIRONMENT
    data_storage_account_name: str #From ENVIRONMENT
    # Prevent the credentials from being printed or logged (using e.g., print() or repr())
    tenant_id: str = Field(repr=False, default="foo") #Ud? #From ENVIRONMENT
    spn_app_id: str = Field(repr=False, default="foo") #Ud? #From ENVIRONMENT
    spn_app_secret: str = Field(repr=False, default="foo") #Ud? #From ENVIRONMENT

    calculation_input_folder_name: str = Field(..., validation_alias=AliasChoices('calculation_input_folder_name','calculation-input-folder-name')) #Ud? From CLI/ENVIRONMENT

    time_series_points_table_name: str | None  = Field(default=None, validation_alias=AliasChoices('time_series_points_table_name','time-series-points-table-name'))  #From CLI
    metering_point_periods_table_name: str | None  = Field(default=None, validation_alias=AliasChoices('metering_point_periods_table_name','metering-point-periods-table-name')) #From CLI
    grid_loss_metering_point_ids_table_name: str | None = Field(default=None, validation_alias=AliasChoices('grid_loss_metering_points_table_name','grid-loss-metering-points-table-name', 'grid_loss_metering_point_ids_table_name')) #From CLI

    data_storage_account_credentials: ClientSecretCredential | None = Field(default=None)
    wholesale_container_path: str | None = Field(default=None)
    calculation_input_path: str | None = Field(default=None)

    def model_post_init(self, __context) -> None:
        self.data_storage_account_credentials = self.data_storage_account_credentials or ClientSecretCredential(
            tenant_id=self.tenant_id,
            client_id=self.spn_app_id,
            client_secret=self.spn_app_secret
        )
        self.wholesale_container_path = self.wholesale_container_path or paths.get_container_root_path(str(self.data_storage_account_name))
        self.calculation_input_path = self.calculation_input_path or paths.get_calculation_input_path(
            str(self.data_storage_account_name),
            str(self.calculation_input_folder_name)
        )



    # @computed_field
    # @property
    # def data_storage_account_credentials(self) -> ClientSecretCredential:
    #     return ClientSecretCredential(tenant_id=self.tenant_id, client_id=self.spn_app_id,
    #                                client_secret=self.spn_app_secret)
    # @computed_field()
    # @property
    # def wholesale_container_path(self) -> str:
    #     return paths.get_container_root_path(str(self.data_storage_account_name))
    #
    # @computed_field
    # @property
    # def calculation_input_path(self) -> str:
    #     return paths.get_calculation_input_path(str(self.data_storage_account_name), str(self.calculation_input_folder_name))



#
#
# infrastructure_settings = InfrastructureSettings(
#         catalog_name="spark_catalog",
#         calculation_input_database_name="wholesale_migrations_wholesale",
#         data_storage_account_name="foo",
#         tenant_id = "foo",
#         spn_app_id = "foo",
#         spn_app_secret = "foo",
#         wholesale_container_path='data_lake_path',
#         calculation_input_path='calculation_input_path',
#         time_series_points_table_name=None,
#         metering_point_periods_table_name=None,
#         grid_loss_metering_point_ids_table_name=None,
#         calculation_input_folder_name = 'foo',
#     )
#
# for s in infrastructure_settings:
#     print(s)


